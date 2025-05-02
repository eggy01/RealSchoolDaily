using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Condition
{
    public enum ConditionType
    {
        Favorability,   // 好感度
        Time,           // 时间
        Scene,          // 场景
        Item,           // 物品
        Quest,          // 任务
        Flag,           // 标记
        Custom          // 自定义条件
    }

    [Header("基础设置")]
    public ConditionType type;
    public bool negate = false; // 是否取反条件

    [Header("通用参数")]
    public string targetKey;    // 目标ID/名称
    public string compareOp;    // 比较运算符
    public int requiredValue;   // 需要比较的值

    [Header("时间专用参数")]
    public string timeValue;    // 时间字符串（如"九月三日19:00"）

    [Header("自定义条件")]
    public string customConditionString; // 原始条件字符串

    /// <summary>
    /// 检查条件是否满足
    /// </summary>
    public bool IsMet()
    {
        bool result = false;

        try
        {
            switch (type)
            {
                case ConditionType.Favorability:
                    result = CheckFavorability();
                    break;

                case ConditionType.Time:
                    result = CheckTime();
                    break;

                case ConditionType.Scene:
                    result = CheckScene();
                    break;

                // case ConditionType.Item:
                //     result = CheckItem();
                //     break;

                // case ConditionType.Quest:
                //     result = CheckQuest();
                //     break;

                // case ConditionType.Flag:
                //     result = CheckFlag();
                //     break;

                case ConditionType.Custom:
                    result = ConditionSystem.Check(customConditionString);
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"条件检查出错: {type} - {e}");
            result = false;
        }

        return negate ? !result : result;
    }

    private bool CheckFavorability()
    {
        int currentValue = FavorabilityManager.Instance?.Get(targetKey) ?? 0;
        return Compare(currentValue, compareOp, requiredValue);
    }

    private bool CheckTime()
    {
        int currentTimeValue = ConditionSystem.GetTimeValue("");
        int requiredTimeValue = ConditionSystem.CalculateTimeWeight(timeValue);
        return Compare(currentTimeValue, compareOp, requiredTimeValue);
    }

    private bool CheckScene()
    {
        int currentSceneValue = ConditionSystem.GetSceneIndex(targetKey);
        return Compare(currentSceneValue, "==", 1); // 场景只需判断是否相等
    }

    // private bool CheckItem()
    // {
    //     int itemCount = InventoryManager.GetItemCount(targetKey);
    //     return Compare(itemCount, compareOp, requiredValue);
    // }

    // private bool CheckQuest()
    // {
    //     int questProgress = QuestSystem.GetQuestProgress(targetKey);
    //     return Compare(questProgress, compareOp, requiredValue);
    // }

    // private bool CheckFlag()
    // {
    //     bool flagState = FlagSystem.GetFlag(targetKey);
    //     return Compare(flagState ? 1 : 0, compareOp, requiredValue);
    // }

    private static bool Compare(int current, string op, int required)
    {
        return op switch
        {
            ">" => current > required,
            ">=" => current >= required,
            "=" or "==" => current == required,
            "<" => current < required,
            "<=" => current <= required,
            "!=" => current != required,
            _ => throw new ArgumentException($"未知运算符: {op}")
        };
    }

    /// <summary>
    /// 将条件转换为系统可识别的字符串格式
    /// </summary>
    public string ToConditionString()
    {
        return type switch
        {
            ConditionType.Favorability => $"好感度.{targetKey}.{compareOp}.{requiredValue}",
            ConditionType.Time => $"时间.{compareOp}.{timeValue}",
            ConditionType.Scene => $"场景.{targetKey}.==.1",
            ConditionType.Item => $"Item.{targetKey}.{compareOp}.{requiredValue}",
            ConditionType.Quest => $"Quest.{targetKey}.{compareOp}.{requiredValue}",
            ConditionType.Flag => $"Flag.{targetKey}.==.{(requiredValue > 0 ? 1 : 0)}",
            _ => customConditionString
        };
    }
}
