using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class RandomEvent
{
    public string eventId;
    [TextArea(1, 3)] public string description;
    public string dialogueName;
    [Range(0, 100)] public int triggerProbability = 100;
    public string condition; // 新增：判定条件表达式
    public bool isImportant;
}

public class RandomEventSystem : MonoBehaviour
{
    private const string CSV_FILE_PATH = "DialogueCSV/RandomEvents/RandomEvents";

    public List<RandomEvent> eventDatabase = new List<RandomEvent>();

    private static RandomEventSystem _instance;
    public static RandomEventSystem Instance => _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadEventsFromCSV();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 从CSV文件加载随机事件
    /// </summary>
    private void LoadEventsFromCSV()
    {
        TextAsset csvFile = Resources.Load<TextAsset>(CSV_FILE_PATH);
        if (csvFile == null)
        {
            Debug.LogError($"CSV文件未找到: {CSV_FILE_PATH}");
            return;
        }


        string[] lines = csvFile.text.Split('\n');
        if (lines.Length <= 1) return; // 跳过标题行

        for (int i = 1; i < lines.Length; i++)
        {

            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] fields = ParseCSVLine(lines[i]);
            // 检查最小字段数要求
            if (fields.Length < 3)
            {
                Debug.LogWarning($"行 {i} 字段不足，跳过处理。字段数: {fields.Length}");
                continue;
            }
            //Debug.Log("随机事件:" + fields[0]);
            RandomEvent newEvent = new RandomEvent
            {
                eventId = fields[0].Trim(),
                description = fields[1].Trim(),
                dialogueName = fields[2].Trim(),
                triggerProbability = int.TryParse(fields[3].Trim(), out int prob) ? prob : 100,
                isImportant = fields.Length > 4 && fields[4].Trim().ToLower() == "true"
            };

            eventDatabase.Add(newEvent);
        }

        Debug.Log($"从CSV成功加载 {eventDatabase.Count} 个随机事件");
    }

    /// <summary>
    /// 从一组事件中随机触发指定次数（不重复）
    /// </summary>
    /// <param name="eventPrefix">事件ID前缀（如"R002"）</param>
    /// <param name="triggerCount">需要触发的次数</param>
    /// <param name="onCompleted">完成回调（参数为是否成功触发）</param>
    public void TriggerRandomEventsFromGroup(string eventPrefix, int triggerCount, System.Action<bool> onCompleted)
    {
        var eligibleEvents = eventDatabase
            .Where(e => e.eventId.StartsWith(eventPrefix))
            .ToList();

        if (eligibleEvents.Count == 0)
        {
            Debug.LogWarning($"没有找到以 {eventPrefix} 开头的事件");
            onCompleted?.Invoke(false);
            return;
        }

        // 直接启动协程并等待完成
        StartCoroutine(TriggerEventsSequentially(eligibleEvents, triggerCount, onCompleted));
    }

    private IEnumerator TriggerEventsSequentially(List<RandomEvent> events, int triggerCount, System.Action<bool> onCompleted)
    {
        int triggeredCount = 0;
        var availableEvents = new List<RandomEvent>(events);
        bool anyTriggered = false;

        while (triggeredCount < triggerCount && availableEvents.Count > 0)
        {
            // 随机选择一个事件
            int randomIndex = Random.Range(0, availableEvents.Count);
            var selectedEvent = availableEvents[randomIndex];
            availableEvents.RemoveAt(randomIndex); // 移除已选事件，避免重复

            bool eventCompleted = false;
            bool eventResult = false;

            // 使用TriggerEventWithCallback确保正确等待事件完成
            TriggerEventWithCallback(selectedEvent.eventId, (result) =>
            {
                eventResult = result;
                eventCompleted = true;
            });

            // 等待事件完成
            yield return new WaitUntil(() => eventCompleted);

            if (eventResult)
            {
                triggeredCount++;
                anyTriggered = true;
                Debug.Log($"成功触发事件: {selectedEvent.eventId} ({triggeredCount}/{triggerCount})");

                // 如果已经触发足够次数，跳出循环
                if (triggeredCount >= triggerCount)
                    break;

                // 等待短暂时间再触发下一个事件
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                Debug.Log($"事件触发失败: {selectedEvent.eventId}");
            }
        }

        onCompleted?.Invoke(anyTriggered);
    }

    /// <summary>
    /// 触发单个随机事件（带回调）
    /// </summary>
    public void TriggerEvent(string eventId, System.Action<bool> onResult)
    {
        var eventData = GetEvent(eventId);
        if (eventData == null)
        {
            Debug.LogWarning($"事件ID不存在: {eventId}");
            onResult?.Invoke(false);
            return;
        }

        bool shouldTrigger = eventData.triggerProbability >= 100 ||
                           Random.Range(0, 100) < eventData.triggerProbability;

        if (shouldTrigger)
        {
            EventHandler.CallStartNewDialogueEvent(eventData.dialogueName, () =>
            {
                onResult?.Invoke(true);
            });
        }
        else
        {
            onResult?.Invoke(false);
        }
    }


    /// <summary>
    /// 触发随机事件并返回判定结果
    /// </summary>
    /// <param name="eventId">事件ID</param>
    /// <param name="onCompleted">事件完成后的回调(参数为是否触发成功)</param>
    public void TriggerEventWithCallback(string eventId, System.Action<bool> onCompleted)
    {
        var eventData = GetEvent(eventId);
        if (eventData == null)
        {
            Debug.LogWarning($"事件ID不存在: {eventId}");
            onCompleted?.Invoke(false);
            return;
        }

        // 检查条件是否满足
        if (!string.IsNullOrEmpty(eventData.condition))
        {
            if (!ConditionSystem.Check(eventData.condition))
            {
                Debug.Log($"事件 {eventId} 条件不满足: {eventData.condition}");
                onCompleted?.Invoke(false);
                return;
            }
        }

        bool shouldTrigger = eventData.triggerProbability >= 100 ||
                           Random.Range(0, 100) < eventData.triggerProbability;

        if (shouldTrigger)
        {
            Debug.Log($"触发事件: {eventId}");

            // 启动黑屏流程协程
            StartCoroutine(ExecuteEventWithBlackScreen(eventData, onCompleted));
        }
        else
        {
            Debug.Log($"事件 {eventId} 概率判定失败");
            onCompleted?.Invoke(false);
        }
    }

    private IEnumerator ExecuteEventWithBlackScreen(RandomEvent eventData, System.Action<bool> onCompleted)
    {
        // 1. 设置黑屏层级
        BlackScreenManager.Instance.TransionBlackScreenSortOrder(100);

        // 2. 淡入黑屏
        yield return BlackScreenManager.Instance.FadeIn(0.5f, false);

        // 3. 设置黑屏文字
        if (eventData.eventId.Equals("R00307"))
            BlackScreenManager.Instance.SetText("你尝试跑路，但被学业导师抓到了，只好乖乖参加军训了");
        else
            BlackScreenManager.Instance.SetText("军训时发生了很多有意思的事情");

        // 4. 等待短暂时间让玩家阅读文字
        yield return new WaitForSeconds(2f);

        // 5. 淡出黑屏
        yield return BlackScreenManager.Instance.FadeOut(0.5f, false);

        // 6. 重置黑屏层级
        BlackScreenManager.Instance.TransionBlackScreenSortOrder(0);

        // 7. 确保黑屏完全淡出后再触发对话
        yield return new WaitForSeconds(0.1f);

        // 8. 触发对话事件
        bool dialogueCompleted = false;
        EventHandler.CallStartNewDialogueEvent(eventData.dialogueName, () =>
        {
            dialogueCompleted = true;
        });

        // 等待对话完成
        yield return new WaitUntil(() => dialogueCompleted);

        // 9. 回调通知完成
        onCompleted?.Invoke(true);
    }

    private RandomEvent GetEvent(string eventId)
    {
        return eventDatabase.Find(e => e.eventId == eventId);
    }

    /// <summary>
    /// 解析CSV行，处理包含逗号的情况
    /// </summary>
    private string[] ParseCSVLine(string line)
    {
        List<string> fields = new List<string>();
        bool inQuotes = false;
        int startIndex = 0;

        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (line[i] == ',' && !inQuotes)
            {
                fields.Add(line.Substring(startIndex, i - startIndex).Replace("\"", ""));
                startIndex = i + 1;
            }
        }

        // 添加最后一个字段
        fields.Add(line.Substring(startIndex).Replace("\"", ""));

        return fields.ToArray();
    }



    /// <summary>
    /// 通过事件ID触发随机事件
    /// </summary>
    /// <param name="eventId">事件ID</param>
    /// <param name="ignoreProbability">是否忽略概率强制触发</param>
    /// <returns>返回对应的剧情key，未触发返回null</returns>
    public bool TriggerEvent(string eventId, bool ignoreProbability = false)
    {
        //var eventData = GetEvent(eventId);
        var eventData = new RandomEvent();
        if (eventData == null)
        {
            Debug.LogWarning($"事件ID不存在: {eventId}");
            return false;
        }

        bool shouldTrigger = ignoreProbability ||
                           eventData.triggerProbability >= 100 ||
                           Random.Range(0, 100) < eventData.triggerProbability;

        if (shouldTrigger)
        {
            //开启对话
            EventHandler.CallStartNewDialogueEvent(eventData.dialogueName);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 检查事件是否存在
    /// </summary>
    public bool HasEvent(string eventId)
    {
        // return System.Array.Exists(eventDatabase, e => e.eventId == eventId);
        return false;
    }
}