using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Condition
{
    public ConditionType type;    // 条件类型
    public string target;        // 目标标识（NPC名称/任务ID/物品ID等）
    public int requiredValue;     // 要求的值
    public CompareOperator comparison; // 比较运算符

    /// <summary>
    /// 构造函数（可选）
    /// </summary>
    public Condition(
        ConditionType type = ConditionType.Favorability,
        string target = "",
        int requiredValue = 0,
        CompareOperator comparison = CompareOperator.GreaterOrEqual)
    {
        this.type = type;
        this.target = target;
        this.requiredValue = requiredValue;
        this.comparison = comparison;
    }
}

/// <summary>
/// 比较运算符枚举
/// </summary>
public enum CompareOperator
{
    GreaterThan,    // >
    Equal,          // ==
    LessThan,       // <
    GreaterOrEqual, // >=
    LessOrEqual,    // <=
    NotEqual       // != （可根据需要添加）
}

/// <summary>
/// 条件类型枚举
/// </summary>
public enum ConditionType
{
    // 基础类型
    Favorability,   // 角色好感度
    QuestProgress,  // 任务进度百分比
    ItemOwned,      // 物品持有数量
    FlagSet,        // 布尔标志（1=true/0=false）

    // 游戏状态
    TimeOfDay,      // 游戏内时间（小时）
    DayOfWeek,      // 星期几
    GamePhase,      // 游戏阶段

    // 扩展类型
    SkillLevel,     // 技能等级
}

/// <summary>
/// 条件逻辑运算符（可选扩展）
/// </summary>
public enum LogicalOperator
{
    All,    // 必须满足所有条件（AND）
    Any     // 满足任意条件即可（OR）
}
