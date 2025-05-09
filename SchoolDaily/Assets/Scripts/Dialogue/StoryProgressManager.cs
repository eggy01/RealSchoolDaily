using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;

public class StoryProgressManager : MonoBehaviour
{ // 单例引用
    public static StoryProgressManager Instance { get; private set; }
    // CSV文件路径
    public TextAsset storyListCSV; // 剧情清单


    // 使用字典存储剧情进度，键为剧情文件名，值为布尔值
    private Dictionary<string, bool> storyProgressDict = new Dictionary<string, bool>();

    // 使用字典存储剧情解锁条件，键为剧情文件名，值为前置剧情文件名
    private Dictionary<string, string> storyUnlockConditions = new Dictionary<string, string>();

    // 使用字典存储剧情时间限制，键为剧情文件名，值为截止时间
    private Dictionary<string, string> storyTimeLimits = new Dictionary<string, string>();

    // 存档文件路径
    // private string saveFilePath;

    private Coroutine timeCheckCoroutine;

    public TextMeshProUGUI storyProgressText; // UI文本框，用于显示剧情进度

    [Header("时间检查设置")]
    [Tooltip("定期检查时间限制的时间间隔（秒）")]
    [SerializeField] private float timeCheckInterval = 1f; // 默认5分钟检查一次


    // 存档数据结构
    [System.Serializable]
    private class SaveData
    {
        public Dictionary<string, bool> progressDict;
        public Dictionary<string, string> unlockConditions;
        public Dictionary<string, string> timeLimits;
        public Dictionary<string, bool> dialogueLineProgress;  // 新增：对话行标记（键格式："文件名_行
    }

    // 确保只有一个实例
    void Awake()
    {
        Instance = this;
        //saveFilePath = Path.Combine(Application.persistentDataPath, "storyProgress.save");
        //Debug.Log("存档路径: " + saveFilePath);

        //InitializeProgressData();

    }
    void OnEnable()
    {
        EventHandler.OnDateChanged += CheckTimeLimitedStories;
    }
    void OnDisable()
    {
        EventHandler.OnDateChanged -= CheckTimeLimitedStories;
    }
    // private void InitializeProgressData()
    // {
    //     if (!LoadProgress())
    //     {
    //         LoadStoryProgressFromCSV();
    //         SaveProgress(); // 初始化后立即保存
    //     }

    //     //StartPeriodicTimeCheck();
    // }

    public void CheckTimeLimitedStories(string date)
    {
        //Debug.LogWarning("订阅日期变化事件");
        //Debug.LogWarning($"剧情截止自动检查，待检查剧情数量: {storyTimeLimits.Count}");
        // Debug.LogError("剧情截止自动检查");
        //bool anyChange = false;
        //string currentTime = TimeManager.Instance.GetCurrentDateTime();

        foreach (var timeLimit in storyTimeLimits)
        {
            string storyID = timeLimit.Key;
            string deadline = timeLimit.Value;

            // 跳过已完成的剧情
            if (storyProgressDict.TryGetValue(storyID, out bool isCompleted) && isCompleted)
                continue;

            // 使用ConditionSystem检查时间条件
            if (ConditionSystem.Check(deadline))
            {
                storyProgressDict[storyID] = true;
                //anyChange = true;
                //Debug.Log($"剧情 [{storyID}] 因超过截止时间 {deadline} 被自动标记为已完成");

                // 触发事件通知其他系统
                //EventHandler.CallStoryAutoCompletedEvent(storyID);
            }
        }

        // if (anyChange)
        // {
        //     SaveProgress();
        // }
    }
    public void setStoryCompleted(string storyID)
    {
        storyProgressDict[storyID] = false;
    }

    public void LoadStoryProgressFromCSV()
    {
        if (storyListCSV == null)
        {
            Debug.LogError("剧情清单CSV文件未分配!");
            return;
        }

        storyProgressDict.Clear();
        storyUnlockConditions.Clear();
        storyTimeLimits.Clear();

        try
        {
            string[] lines = storyListCSV.text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 1; i < lines.Length; i++) // 跳过标题行
            {
                string[] parts = lines[i].Split(',');
                if (parts.Length == 0) continue;

                string storyID = parts[0].Trim();
                if (string.IsNullOrEmpty(storyID)) continue;

                // 初始化进度
                storyProgressDict[storyID] = false;

                // 解析解锁条件
                if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1].Trim()))
                {
                    storyUnlockConditions[storyID] = parts[1].Trim();
                }

                // 解析时间限制
                if (parts.Length > 2 && !string.IsNullOrEmpty(parts[2].Trim()))
                {
                    storyTimeLimits[storyID] = parts[2].Trim();
                }
            }
            Debug.Log("剧情文件加载完成");
        }
        catch (Exception e)
        {
            Debug.LogError($"解析CSV失败: {e.Message}");
        }
    }

    public void SaveProgress(string saveFilePath)
    {
        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(saveFilePath, FileMode.Create))
            {
                SaveData data = new SaveData
                {
                    progressDict = storyProgressDict,
                    unlockConditions = storyUnlockConditions,
                    timeLimits = storyTimeLimits,
                    dialogueLineProgress = dialogueLineProgress // 新增
                };
                formatter.Serialize(stream, data);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"存档失败: {e.Message}");
        }
    }

    public void LoadProgress(string saveFilePath)
    {
        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(saveFilePath, FileMode.Open))
            {
                SaveData data = formatter.Deserialize(stream) as SaveData;
                if (data != null)
                {
                    storyProgressDict = data.progressDict ?? new Dictionary<string, bool>();
                    storyUnlockConditions = data.unlockConditions ?? new Dictionary<string, string>();
                    storyTimeLimits = data.timeLimits ?? new Dictionary<string, string>();
                    dialogueLineProgress = data.dialogueLineProgress ?? new Dictionary<string, bool>(); // 新增
                    return;
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log($"读档失败: {e.Message}");
            LoadStoryProgressFromCSV();
        }
        return;
    }


    // 更新UI显示的剧情进度和解锁条件
    public void UpdateStoryProgressUI()
    {
        if (storyProgressText != null)
        {
            storyProgressText.text = "剧情进度:\n";
            foreach (var story in storyProgressDict)
            {
                storyProgressText.text += story.Key + " - " + (story.Value ? "已完成\n" : "未完成\n");
            }
        }
    }

    // 标记剧情为已过
    public void MarkStoryAsCompleted(string storyFileName)
    {
        if (storyProgressDict.ContainsKey(storyFileName))
        {
            storyProgressDict[storyFileName] = true;
            //SaveProgress(); // 自动保存
        }
        else
        {
            Debug.LogError("Story file name not found: " + storyFileName);
        }
    }

    // 检查剧情是否已过
    public bool IsStoryCompleted(string storyFileName)
    {
        if (storyProgressDict.ContainsKey(storyFileName))
        {
            return storyProgressDict[storyFileName];
        }
        else
        {
            Debug.LogError("Story file name not found: " + storyFileName);
            return false;
        }
    }

    // 检查剧情是否可以解锁
    public bool CanUnlockStory(string storyFileName)
    {
        if (storyUnlockConditions.ContainsKey(storyFileName))
        {
            string previousStoryFileName = storyUnlockConditions[storyFileName];
            return IsStoryCompleted(previousStoryFileName);
        }
        else
        {
            return true;
        }
    }


    // 检查并更新所有有时间限制的剧情状态


    // 添加新的剧情
    public void AddNewStory(string storyFileName, string previousStoryFileName = "")
    {
        // 检查剧情文件是否已经存在
        if (!storyProgressDict.ContainsKey(storyFileName))
        {
            // 如果剧情文件不存在，直接添加
            storyProgressDict.Add(storyFileName, false);
            Debug.Log("New story added with file name: " + storyFileName);
        }
        else
        {
            Debug.Log("剧情文件已存在: " + storyFileName);
        }

        // 检查前置条件是否需要添加
        if (!string.IsNullOrEmpty(previousStoryFileName))
        {
            if (!storyUnlockConditions.ContainsKey(storyFileName))
            {
                // 如果剧情文件没有前置条件，添加前置条件
                storyUnlockConditions.Add(storyFileName, previousStoryFileName);
                Debug.Log("已添加前置条件: " + previousStoryFileName + " -> " + storyFileName);
            }
            else
            {
                // 如果剧情文件已经有前置条件，输出日志
                Debug.Log("剧情文件 " + storyFileName + " 已有前置条件: " + storyUnlockConditions[storyFileName]);
            }
        }
    }

    public bool IsDialogueCompleted(int dialogueID)
    {
        return PlayerPrefs.GetInt($"DialogueCompleted_{dialogueID}", 0) == 1;
    }

    public void MarkDialogueCompleted(int dialogueID)
    {
        PlayerPrefs.SetInt($"DialogueCompleted_{dialogueID}", 1);
        PlayerPrefs.Save();
    }

    // 打印剧情进度和解锁条件到控制台
    // public void PrintStoryProgress()
    // {
    //     Debug.Log("剧情进度:");
    //     foreach (var story in storyProgressDict)
    //     {
    //         Debug.Log(story.Key + " - " + (story.Value ? "已完成" : "未完成"));
    //     }

    //     Debug.Log("剧情解锁条件:");
    //     foreach (var condition in storyUnlockConditions)
    //     {
    //         Debug.Log(condition.Key + " -> " + condition.Value);
    //     }
    // }

    // 存储对话行完成状态的字典
    private Dictionary<string, bool> dialogueLineProgress = new Dictionary<string, bool>();

    // 生成对话行的唯一键（格式：文件名_行号）
    private string GetDialogueLineKey(string storyFileName, int lineNumber)
    {
        return $"{storyFileName}_{lineNumber}";
    }

    // 检查某行是否已完成
    public bool IsDialogueLineCompleted(string storyFileName, int lineNumber)
    {
        string key = GetDialogueLineKey(storyFileName, lineNumber);
        return dialogueLineProgress.TryGetValue(key, out bool completed) && completed;
    }

    // 标记某行为已完成
    public void MarkDialogueLineCompleted(string storyFileName, int lineNumber)
    {
        string key = GetDialogueLineKey(storyFileName, lineNumber);
        dialogueLineProgress[key] = true;
        //SaveProgress(); // 自动保存到二进制文件

#if UNITY_EDITOR
        Debug.Log($"标记对话行完成: {key}");
#endif
    }

    // 重置某文件的所有行标记
    public void ResetDialogueLines(string storyFileName)
    {
        var keysToRemove = dialogueLineProgress.Keys
            .Where(k => k.StartsWith(storyFileName + "_"))
            .ToList();

        foreach (var key in keysToRemove)
        {
            dialogueLineProgress.Remove(key);
        }
        //SaveProgress();
    }
}
