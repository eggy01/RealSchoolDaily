using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using SchoolD.Dialogue;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    private string saveFilePath;

    public TextMeshProUGUI storyProgressText; // UI文本框，用于显示剧情进度


    // 存档数据结构
    [System.Serializable]
    private class SaveData
    {
        public Dictionary<string, bool> progressDict;
        public Dictionary<string, string> unlockConditions;
    }

    // 确保只有一个实例
    void Awake()
    {
        Instance = this;
        saveFilePath = Path.Combine(Application.persistentDataPath, "storyProgress.save");
        Debug.Log("存档路径: " + saveFilePath);
        // 先尝试加载，如果失败再从CSV初始化
        if (!LoadProgress())
        {
            LoadStoryProgressFromCSV();
        }


    }
    private void LoadStoryProgressFromCSV()
    {
        if (storyListCSV == null)
        {
            Debug.LogError("Story list CSV file not assigned!");
            return;
        }

        // 清空现有数据
        storyProgressDict.Clear();
        storyUnlockConditions.Clear();
        storyTimeLimits.Clear();

        // 解析CSV
        string[] lines = storyListCSV.text.Split('\n');

        // 跳过标题行（如果有）
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            // 分割CSV行
            string[] parts = line.Split(',');
            if (parts.Length == 0) continue;

            // 获取剧情ID（第1列）
            string storyID = parts[0].Trim();
            if (string.IsNullOrEmpty(storyID)) continue;

            // 添加到进度字典（默认未完成）
            if (!storyProgressDict.ContainsKey(storyID))
            {
                storyProgressDict.Add(storyID, false);
                Debug.Log($"添加剧情: {storyID}");
            }

            // 如果有解锁条件（第2列），则添加到解锁条件字典
            if (parts.Length >= 2 && !string.IsNullOrEmpty(parts[1].Trim()))
            {
                string previousStoryID = parts[1].Trim();
                storyUnlockConditions[storyID] = previousStoryID;
                Debug.Log($"设置解锁条件: {storyID} 需要先完成 {previousStoryID}");
            }

            // 如果有时间限制（第3列），则解析并添加到时间限制字典
            if (parts.Length >= 3 && !string.IsNullOrEmpty(parts[2].Trim()))
            {
                string timeLimitStr = parts[2].Trim();
                try
                {
                    storyTimeLimits[storyID] = timeLimitStr;
                    Debug.Log($"设置时间限制: {storyID} 截止时间 {timeLimitStr}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"解析时间限制失败: {timeLimitStr}, 错误: {e.Message}");
                }
            }
        }
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
            SaveProgress(); // 自动保存
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
    public void CheckTimeLimitedStories()
    {

        foreach (var timeLimit in storyTimeLimits)
        {

            string storyID = timeLimit.Key;
            string deadline = timeLimit.Value;

            // 如果当前时间已经超过截止时间，且剧情尚未完成
            if (ConditionSystem.Check(deadline) && !storyProgressDict[storyID])
            {
                storyProgressDict[storyID] = true;
                Debug.Log($"剧情 {storyID} 因超过截止时间 {deadline} 被自动标记为已完成");
            }
        }

        SaveProgress(); // 自动保存变更
    }


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
    // 打印剧情进度和解锁条件到控制台
    public void PrintStoryProgress()
    {
        Debug.Log("剧情进度:");
        foreach (var story in storyProgressDict)
        {
            Debug.Log(story.Key + " - " + (story.Value ? "已完成" : "未完成"));
        }

        Debug.Log("剧情解锁条件:");
        foreach (var condition in storyUnlockConditions)
        {
            Debug.Log(condition.Key + " -> " + condition.Value);
        }
    }

    // 保存进度到文件
    public void SaveProgress()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(saveFilePath, FileMode.Create);

        SaveData data = new SaveData();
        data.progressDict = storyProgressDict;
        data.unlockConditions = storyUnlockConditions;

        formatter.Serialize(stream, data);
        stream.Close();
        Debug.Log("已存档 ");
    }

    // 从文件加载进度
    public bool LoadProgress()
    {
        if (File.Exists(saveFilePath))
        {
            Debug.Log("存在路径");
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(saveFilePath, FileMode.Open);

            try
            {
                SaveData data = formatter.Deserialize(stream) as SaveData;
                stream.Close();

                if (data != null)
                {
                    storyProgressDict = data.progressDict;
                    storyUnlockConditions = data.unlockConditions;
                    return true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("加载存档失败: " + e.Message);
                stream.Close();
            }
        }
        return false;
    }

    // 删除存档(用于测试)
    public void DeleteSaveFile()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("存档已删除");
        }
    }
}
