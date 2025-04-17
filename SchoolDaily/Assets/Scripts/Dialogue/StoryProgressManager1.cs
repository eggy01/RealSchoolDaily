using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryProgressManager1 : MonoBehaviour
{ // 单例引用
    public static StoryProgressManager1 Instance { get; private set; }

    // 使用字典存储剧情进度，键为剧情文件名，值为布尔值
    private Dictionary<string, bool> storyProgressDict = new Dictionary<string, bool>();

    // 使用字典存储剧情解锁条件，键为剧情文件名，值为前置剧情文件名
    private Dictionary<string, string> storyUnlockConditions = new Dictionary<string, string>();

    // 确保只有一个实例
    void Awake()
    {
        Instance = this;

        // 初始化剧情进度和解锁条件
        InitializeStoryProgress();
    }

    private void InitializeStoryProgress()
    {
        // 初始化剧情进度字典
        // 示例：添加初始剧情进度
        storyProgressDict.Add("Beginner_01", false);
        storyProgressDict.Add("Beginner_02", false);
        storyProgressDict.Add("Beginner_03", false);
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
            Debug.Log("dafferre:" + storyFileName);
            return false;
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
}
