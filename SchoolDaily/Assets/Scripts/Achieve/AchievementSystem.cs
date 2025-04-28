using UnityEngine;
using System.Collections.Generic;

public class AchievementSystem : MonoBehaviour
{
    public static AchievementSystem Instance { get; private set; }

    // 存储成就进度
    private Dictionary<string, int> achievementProgress = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 增加成就进度
    public void IncrementAchievementProgress(string achievementID, int amount = 1)
    {
        if (achievementProgress.ContainsKey(achievementID))
        {
            achievementProgress[achievementID] += amount;
        }
        else
        {
            achievementProgress.Add(achievementID, amount);
        }

        Debug.Log($"成就进度更新: {achievementID} - 当前进度: {achievementProgress[achievementID]}");
    }

    // 检查是否达成成就
    public bool CheckAchievement(string achievementID, int requiredCount)
    {
        if (achievementProgress.TryGetValue(achievementID, out int currentCount))
        {
            return currentCount >= requiredCount;
        }
        return false;
    }

    // 解锁成就
    public void UnlockAchievement(string achievementID)
    {
        Debug.Log($"成就解锁: {achievementID}");
        // 这里可以添加成就解锁后的效果，比如显示UI、播放音效等
    }
}
