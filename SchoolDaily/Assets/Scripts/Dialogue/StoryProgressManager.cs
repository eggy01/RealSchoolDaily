using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class StoryProgressManager : MonoBehaviour
{ // 单例引用
    public static StoryProgressManager Instance { get; private set; }

    // 使用字典存储剧情进度，键为剧情文件名，值为布尔值
    private Dictionary<string, bool> storyProgressDict = new Dictionary<string, bool>();

    // 使用字典存储剧情解锁条件，键为剧情文件名，值为前置剧情文件名
    private Dictionary<string, string> storyUnlockConditions = new Dictionary<string, string>();

    // 存档文件路径
    private string saveFilePath;

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

        // 初始化剧情进度和解锁条件
        InitializeStoryProgress();

        PrintStoryProgress();

    }

    public void InitializeStoryProgress()
    {
        // 初始化剧情进度字典
        // 示例：添加初始剧情进度
        storyProgressDict.Add("Beginner_01", false);
        storyProgressDict.Add("Beginner_02", false);
        storyProgressDict.Add("Beginner_03", false);
        storyProgressDict.Add("Beginner_04", false);
        storyProgressDict.Add("Beginner_05", false);
        storyProgressDict.Add("Beginner_06", false);
        storyProgressDict.Add("1111", false);
        storyProgressDict.Add("DefaultInterActive", false);
    }

    // 标记剧情为已过
    public void MarkStoryAsCompleted(string storyFileName)
    {
        if (storyProgressDict.ContainsKey(storyFileName))
        {
            storyProgressDict[storyFileName] = true;
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
    }

    // 从文件加载进度
    public bool LoadProgress()
    {
        if (File.Exists(saveFilePath))
        {
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
