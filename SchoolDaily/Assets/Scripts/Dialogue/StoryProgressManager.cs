using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryProgressManager : MonoBehaviour
{
    // 单例引用
    public static StoryProgressManager Instance { get; private set; }

    // 使用列表存储剧情进度，索引为剧情序号，值为布尔值
    public List<bool> storyProgressList = new List<bool>();

    public int capacity = 20;// 预设容量为10

    // 确保只有一个实例
    void Awake()
    {
        Instance = this;
        InitializeStoryProgress();
    }

    private void InitializeStoryProgress()
    {
        for (int i = 0; i < capacity; i++)
        {
            storyProgressList.Add(false);
        }
    }


    // 标记剧情为已过
    public void MarkStoryAsCompleted(int storyId)
    {
        if (storyId >= 0 && storyId < storyProgressList.Count)
        {
            storyProgressList[storyId] = true;
        }
        else
        {
            Debug.LogError("Story ID out of range: " + storyId);
        }
    }

    // 检查剧情是否已过
    public bool IsStoryCompleted(int storyId)
    {
        Debug.Log("剧情列表数量：" + storyProgressList.Count);
        if (storyId >= 0 && storyId < storyProgressList.Count)
        {
            return storyProgressList[storyId];
        }
        else
        {
            Debug.Log("Story ID out of range: " + storyId);
            return false;
        }
    }

    // 添加新的剧情
    public void AddNewStory()
    {
        storyProgressList.Add(false);
        Debug.Log("New story added.");
    }
}
