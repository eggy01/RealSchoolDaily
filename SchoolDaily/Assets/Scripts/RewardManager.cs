using System;
using System.Text.RegularExpressions;
using UnityEngine;

public class RewardManager : MonoBehaviour
{
    // Singleton instance
    public static RewardManager Instance { get; private set; }

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


    /// <summary>
    /// Applies all rewards from the description string
    /// </summary>
    /// <param name="desc">Reward description string</param>
    public void ApplyRewards(string desc)
    {
        if (string.IsNullOrEmpty(desc)) return;

        // Split by either Chinese or English period
        string[] rewards = desc.Split(new[] { '。' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string reward in rewards)
        {
            string trimmedReward = reward.Trim();
            if (string.IsNullOrEmpty(trimmedReward)) continue;

            try
            {
                // Process each reward type
                if (TryProcessFavorability(trimmedReward)) continue;
                if (TryProcessItem(trimmedReward)) continue;
                if (TryProcessStat(trimmedReward, "体力", (amount) => PlayerInformation.Instance.AddStrength(amount))) continue;
                if (TryProcessStat(trimmedReward, "信念", (amount) => PlayerInformation.Instance.AddStrength(amount))) continue;
                if (TryProcessStat(trimmedReward, "金钱", (amount) => PlayerInformation.Instance.AddGold(amount))) continue;
                if (TryProcessStat(trimmedReward, "声望", (amount) => PlayerInformation.Instance.AddFame(amount))) continue;
                if (TryProcessStat(trimmedReward, "心情", (amount) => PlayerInformation.Instance.AddMood(amount))) continue;
                if (TryProcessStat(trimmedReward, "智力", (amount) => PlayerInformation.Instance.AddIntelligence(amount))) continue;
                // if (TryProcessAcademic(trimmedReward, "成绩", (subject, amount) => AcademicManager.Instance.AddGrade(subject, amount))) continue;
                // if (TryProcessAcademic(trimmedReward, "平时分", (subject, amount) => AcademicManager.Instance.AddRegularScore(subject, amount))) continue;
                // if (TryProcessSkill(trimmedReward)) continue;

                Debug.LogWarning($"Unrecognized reward format: {trimmedReward}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error processing reward '{trimmedReward}': {e.Message}");
            }
        }
    }

    //处理奖励
    private bool TryProcessFavorability(string reward)
    {
        // 格式: "林风.好感度+5"
        if (!reward.Contains(".好感度+")) return false;

        string[] parts = reward.Split(new[] { '.', '+' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3) return false;

        string npcName = parts[0].Trim();
        if (int.TryParse(Regex.Match(parts[2], @"\d+").Value, out int amount))
        {
            FavorabilityManager.Instance.Add(npcName, amount);
            Debug.Log($"增加 {npcName} 的好感度 {amount} 点");

            TipController.Instance.ShowTip(npcName + "好感度" + amount, 4);
            return true;
        }

        Debug.LogWarning($"无法解析好感度数值: {reward}");
        return false;
    }
    //处理物品奖励
    private bool TryProcessItem(string reward)
    {
        // 格式: "物品.苹果+3"
        if (!reward.StartsWith("物品.")) return false;

        string[] parts = reward.Split(new[] { '.', '+' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return false;

        string itemName = parts[1].Trim();
        int amount = 1; // 默认数量为1

        // 如果有数量部分
        if (parts.Length >= 3)
        {
            if (!int.TryParse(parts[2].Trim(), out amount))
            {
                Debug.LogWarning($"无法解析物品数量: {reward}");
                return false;
            }
        }

        PackageLocalData.Instance.AddItem(itemName, amount);//添加物品

        TipController.Instance.ShowTip("获得新物品", 4);
        Debug.Log($"获得物品: {itemName} x{amount}");
        return true;
    }
    //处理玩家属性奖励
    private bool TryProcessStat(string reward, string statName, Action<int> applyReward)
    {
        if (!reward.StartsWith(statName + "+")) return false;

        string[] parts = reward.Split('+');
        if (parts.Length < 2) return false;

        if (int.TryParse(Regex.Match(parts[1], @"\d+").Value, out int amount))
        {
            applyReward(amount);
            Debug.Log($"Added {amount} to {statName}");
            return true;
        }

        Debug.LogWarning($"Failed to parse {statName} amount: {reward}");
        return false;
    }

    private bool TryProcessAcademic(string reward, string rewardType, Action<string, int> applyReward)
    {
        if (!reward.Contains(rewardType + "+")) return false;

        string[] parts = reward.Split('+');
        if (parts.Length < 2) return false;

        string subjectName = parts[0].Replace(rewardType, "").Trim();
        if (int.TryParse(Regex.Match(parts[1], @"\d+").Value, out int amount))
        {
            applyReward(subjectName, amount);
            Debug.Log($"Added {amount} to {subjectName}'s {rewardType}");
            return true;
        }

        Debug.LogWarning($"Failed to parse {rewardType} amount: {reward}");
        return false;
    }

    // private bool TryProcessSkill(string reward)
    // {
    //     if (!reward.StartsWith("技能.")) return false;

    //     string[] parts = reward.Split('.');
    //     if (parts.Length < 2) return false;

    //     string skillName = parts[1].Trim();
    //     SkillManager.Instance.UnlockSkill(skillName);
    //     Debug.Log($"Unlocked skill: {skillName}");
    //     return true;
    // }


    #region Individual Reward Methods (Alternative API)

    public void AddFavorability(string npcName, int amount)
    {
        FavorabilityManager.Instance.Add(npcName, amount);
    }

    public void AddItem(string itemName, int amount = 1)
    {
        PackageLocalData.Instance.AddItem(itemName, amount);
    }

    // public void AddStamina(int amount)
    // {
    //     PlayerStats.Instance.Stamina += amount;
    // }

    // public void AddFaith(int amount)
    // {
    //     PlayerStats.Instance.Faith += amount;
    // }

    // public void AddMoney(int amount)
    // {
    //     PlayerStats.Instance.Money += amount;
    // }

    // public void AddReputation(int amount)
    // {
    //     PlayerStats.Instance.Reputation += amount;
    // }

    // public void AddGrade(string subject, int amount)
    // {
    //     AcademicManager.Instance.AddGrade(subject, amount);
    // }

    // public void AddRegularScore(string subject, int amount)
    // {
    //     AcademicManager.Instance.AddRegularScore(subject, amount);
    // }

    // public void UnlockSkill(string skillName)
    // {
    //     SkillManager.Instance.UnlockSkill(skillName);
    // }

    #endregion
}
