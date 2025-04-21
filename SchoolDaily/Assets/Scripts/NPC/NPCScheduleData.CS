using UnityEngine;

[System.Serializable]
public class ScheduleEntry
{
    public int startHour;    // 开始小时（24小时制）
    public int endHour;      // 结束小时
    public Vector2 targetPosition; // 目标位置
    public bool isLoopPath = true; // 是否循环路径
}

[System.Serializable]
public class SpecialScheduleEntry
{
    public int month;        // 触发月份
    public int day;          // 触发日期
    public int startHour;    // 开始小时
    public int endHour;      // 结束小时
    public Vector2 targetPosition; // 特殊目标位置
}

public class NPCScheduleData : MonoBehaviour
{
    public ScheduleEntry[] dailySchedule;    // 日常行程
    public SpecialScheduleEntry[] specialSchedule; // 特殊日期行程
}