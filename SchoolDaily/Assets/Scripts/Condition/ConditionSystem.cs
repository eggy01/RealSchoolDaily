using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SchoolD.Dialogue;

/// <summary>
/// 静态条件检查服务
/// 使用示例：ConditionSystem.Check("Favorability.林风.>=.30")
/// 时间条件示例：ConditionSystem.Check("时间.>=.九月三日19:00")
/// </summary>
public static class ConditionSystem
{
    // 条件类型到处理函数的映射
    private static readonly Dictionary<string, Func<string, int>> _valueGetters =
        new Dictionary<string, Func<string, int>>(StringComparer.OrdinalIgnoreCase)
        {
            ["好感度"] = GetFavorability,
            ["时间"] = GetTimeValue,  // 时间类型不需要目标参数
            ["场景"] = GetSceneIndex,  // 新增场景判断
                                     // ["QuestProgress"] = GetQuestProgress,
                                     // ["ItemOwned"] = GetItemCount,
                                     //["FlagSet"] = GetFlagState
        };

    /// <summary>
    /// 检查单个条件字符串
    /// </summary>
    public static bool Check(string condition)
    {
        //Debug.Log($"=== 开始处理条件: {condition} ===");
        if (string.IsNullOrWhiteSpace(condition))
        {
            Debug.Log("条件为空，默认通过");
            return true;
        }

        try
        {
            string[] parts = condition.Trim().Split('.');

            string op;
            string timeValueStr;
            int currentTimeValue;
            int requiredTimeValue;

            // 特殊处理时间条件（格式：时间.运算符.值）
            if (parts[0] == "时间" && parts.Length == 3)
            {
                op = parts[1];
                timeValueStr = parts[2];
                currentTimeValue = GetTimeValue(timeValueStr);
                requiredTimeValue = CalculateTimeWeight(timeValueStr);
                //         Debug.Log($"比较详情: 当前值={currentTimeValue}({TimeManager.Instance.GetMonth()}月{TimeManager.Instance.GetDay()}日 " +
                //   $"{TimeManager.Instance.GetHour()}:{TimeManager.Instance.GetMinute()}), " +
                //   $"条件值={timeValueStr}+ :::={requiredTimeValue}, 操作符={op}, 结果={Compare(currentTimeValue, op, requiredTimeValue)}");

                return Compare(currentTimeValue, op, requiredTimeValue);
            }

            // 常规条件处理（格式：类型.目标.运算符.值）
            if (parts.Length != 4) throw new FormatException("条件格式应为：类型.目标.运算符.值 或 时间.运算符.值");

            string type = parts[0];
            string target = parts[1];
            op = parts[2];
            int requiredValue = int.Parse(parts[3]);

            if (!_valueGetters.TryGetValue(type, out var getter))
                throw new ArgumentException($"未知条件类型: {type}");

            int currentValue = getter(target);

            return Compare(currentValue, op, requiredValue);
        }
        catch (Exception e)
        {
            Debug.LogError($"条件检查失败: {condition}\n{e}");
            return false;
        }
    }

    /// <summary>
    /// 检查多个条件（需全部满足）
    /// </summary>
    public static bool CheckAll(string multiConditions)
    {
        if (string.IsNullOrWhiteSpace(multiConditions))
            return true;

        return multiConditions.Split(';')
            .All(cond => Check(cond.Trim()));
    }

    // 核心比较逻辑 --------------------------------------------------
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

    // 数据获取方法（需根据你的游戏系统实现）--------------------------
    private static int GetFavorability(string npcName)
    {
        // 实际接入你的好感度系统
        return FavorabilityManager.Instance?.Get(npcName) ?? 0;
    }

    // 新增场景判断方法
    private static int GetSceneIndex(string sceneName)
    {
        // 方法1：使用场景名称判断
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == sceneName ? 1 : 0;

        // 方法2：或者使用构建索引判断（如果你有固定的场景索引）
        // return UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
    }

    // 在CalculateTimeWeight方法前添加场景相关方法
    private static int GetCurrentSceneBuildIndex()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
    }

    private static int GetTimeValue(string timeStr)
    {
        try
        {
            var timeManager = TimeManager.Instance;
            int currentMonth = timeManager.GetMonth();
            int currentDay = timeManager.GetDay();
            int currentHour = timeManager.GetHour();
            int currentMinute = timeManager.GetMinute();
            //Debug.Log($"当前时间: {currentMonth}月{currentDay}日 {currentHour}:{currentMinute}");

            // 确保和 CalculateTimeWeight 的计算方式一致
            return currentMonth * 1000000 + currentDay * 10000 + currentHour * 100 + currentMinute;
        }
        catch (Exception e)
        {
            Debug.LogError($"获取当前时间失败\n错误: {e}");
            return 0;
        }
    }

    private static int CalculateTimeWeight(string timeStr)
    {
        try
        {
            string[] timeParts;
            string timePart;
            int hour;
            int minute;
            int month;
            int day;

            // 新增：处理完整日期时间格式（如"2月1日19:00"）
            if (timeStr.Contains("月") && timeStr.Contains("日"))
            {
                int monthEnd = timeStr.IndexOf('月');
                int dayEnd = timeStr.IndexOf('日');

                month = int.Parse(timeStr.Substring(0, monthEnd).Trim());
                day = int.Parse(timeStr.Substring(monthEnd + 1, dayEnd - monthEnd - 1).Trim());

                timePart = timeStr.Substring(dayEnd + 1).Trim();
                timeParts = timePart.Split(':');

                hour = int.Parse(timeParts[0]);
                minute = int.Parse(timeParts[1]);

                return month * 1000000 + day * 10000 + hour * 100 + minute;
            }

            // 处理纯时间格式（如 "19:00"）
            if (timeStr.Contains(":") && !timeStr.Contains("月") && !timeStr.Contains("日"))
            {
                var timeManager = TimeManager.Instance;
                int currentMonth = timeManager.GetMonth();
                int currentDay = timeManager.GetDay();

                // 解析纯时间部分
                timeParts = timeStr.Split(':');
                if (timeParts.Length != 2) throw new FormatException("时间部分格式应为HH:MM");

                hour = int.Parse(timeParts[0]);
                minute = int.Parse(timeParts[1]);

                // 使用当前月日 + 指定时间
                return currentMonth * 1000000 + currentDay * 10000 + hour * 100 + minute;
            }

            // 原有完整日期时间处理逻辑
            int monthEndIndex = timeStr.IndexOf('月');
            int dayEndIndex = timeStr.IndexOf('日');

            // 处理无月份的情况（如 "三日19:00"）
            if (monthEndIndex == -1)
            {
                month = TimeManager.Instance.GetMonth();
                dayEndIndex = timeStr.IndexOf('日');
            }
            else
            {
                // 解析月份（如"九月"→9）
                string monthStr = timeStr.Substring(0, monthEndIndex);
                month = ParseChineseMonth(monthStr);
            }

            // 处理无日期的情况（如 "19:00"）
            if (dayEndIndex == -1)
            {
                day = TimeManager.Instance.GetDay();
            }
            else
            {
                // 解析日期（如"三日"→3）
                string dayStr = timeStr.Substring(monthEndIndex + 1, dayEndIndex - monthEndIndex - 1);
                day = ParseChineseNumber(dayStr);
            }

            // 解析时间部分
            timePart = dayEndIndex == -1 ?
                timeStr : // 无日期的情况直接使用整个字符串
                timeStr.Substring(dayEndIndex + 1); // 有日期的情况截取后面部分

            timeParts = timePart.Split(':');
            if (timeParts.Length != 2) throw new FormatException("时间部分格式应为HH:MM");

            hour = int.Parse(timeParts[0]);
            minute = int.Parse(timeParts[1]);

            return month * 1000000 + day * 10000 + hour * 100 + minute;
        }
        catch (Exception e)
        {
            Debug.LogError($"时间条件解析失败: {timeStr}\n错误: {e}");
            return 0;
        }
    }


    // 中文数字转阿拉伯数字（处理"三"→3等）
    private static int ParseChineseNumber(string chineseNumber)
    {
        var numbers = new Dictionary<string, int>
    {
        {"一", 1}, {"二", 2}, {"三", 3}, {"四", 4},
        {"五", 5}, {"六", 6}, {"七", 7}, {"八", 8},
        {"九", 9}, {"十", 10},
        {"十一", 11}, {"十二", 12}, {"十三", 13}, {"十四", 14},
        {"十五", 15}, {"十六", 16}, {"十七", 17}, {"十八", 18},
        {"十九", 19}, {"二十", 20}, {"二十一", 21}, {"二十二", 22},
        {"二十三", 23}, {"二十四", 24}, {"二十五", 25}, {"二十六", 26},
        {"二十七", 27}, {"二十八", 28}, {"二十九", 29}, {"三十", 30},
        {"三十一", 31}
    };

        return numbers.TryGetValue(chineseNumber, out int num) ? num : 0;
    }

    // 中文月份转数字（补充完整你的月份字典）
    private static int ParseChineseMonth(string monthStr)
    {
        var months = new Dictionary<string, int>
            {
                {"一月", 1}, {"二月", 2}, {"三月", 3}, {"四月", 4},
                {"五月", 5}, {"六月", 6}, {"七月", 7}, {"八月", 8},
                {"九月", 9}, {"十月", 10}, {"十一月", 11}, {"十二月", 12}
            };
        return months.TryGetValue(monthStr, out int m) ? m : 0;
    }
}