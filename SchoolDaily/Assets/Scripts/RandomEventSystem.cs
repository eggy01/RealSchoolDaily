using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RandomEvent
{
    public string eventId;          // 事件ID (如 R00101)
    [TextArea(1, 3)]
    public string description;      // 事件描述文本
    public string dialogueName;      // 对应的剧情key (如 randomEventDialogue_1)
    [Range(0, 100)]
    public int triggerProbability = 100;  // 触发概率(0-100)，默认100%
    public bool isImportant = false; // 是否重要事件(触发后记录日志)
}

public class RandomEventSystem : MonoBehaviour
{
    private const string CSV_FILE_PATH = "Events/RandomEvents";

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
            if (fields.Length < 4) continue;

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
    public void TriggerEvent(string eventId, bool ignoreProbability = false)
    {
        var eventData = GetEvent(eventId);
        if (eventData == null)
        {
            Debug.LogWarning($"事件ID不存在: {eventId}");
            return;
        }

        bool shouldTrigger = ignoreProbability ||
                           eventData.triggerProbability >= 100 ||
                           Random.Range(0, 100) < eventData.triggerProbability;

        if (shouldTrigger)
        {
            //开启对话
            EventHandler.CallStartNewDialogueEvent(eventData.dialogueName);
            return;
        }
    }


    /// <summary>
    /// 获取事件完整数据
    /// </summary>
    public RandomEvent GetEvent(string eventId)
    {
        //return System.Array.Find(eventDatabase, e => e.eventId == eventId);
        return;
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