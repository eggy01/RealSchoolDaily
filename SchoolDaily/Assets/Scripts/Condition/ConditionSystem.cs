using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SchoolD.Dialogue
{
    // <summary>
    /// 静态条件检查服务
    /// 使用示例：ConditionSystem.Check("Favorability.林风.>=.30")
    /// </summary>
    public static class ConditionSystem
    {
        // 条件类型到处理函数的映射
        private static readonly Dictionary<string, Func<string, int>> _valueGetters =
            new Dictionary<string, Func<string, int>>(StringComparer.OrdinalIgnoreCase)
            {
                ["Favorability"] = GetFavorability,
                // ["QuestProgress"] = GetQuestProgress,
                // ["ItemOwned"] = GetItemCount,
                //["FlagSet"] = GetFlagState
            };

        /// <summary>
        /// 检查单个条件字符串
        /// </summary>
        public static bool Check(string condition)
        {
            if (string.IsNullOrWhiteSpace(condition))
                return true;

            try
            {
                string[] parts = condition.Trim().Split('.');
                if (parts.Length != 4) throw new FormatException("条件格式应为：类型.目标.运算符.值");

                string type = parts[0];
                string target = parts[1];
                string op = parts[2];
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

        // private static int GetQuestProgress(string questId)
        // {
        //     // 实际接入你的任务系统
        //     return QuestManager.Instance?.GetProgress(questId) ?? 0;
        // }

        // private static int GetItemCount(string itemId)
        // {
        //     // 实际接入你的背包系统
        //     return InventoryManager.Instance?.GetItemCount(itemId) ?? 0;
        // }

        // private static int GetFlagState(string flagName)
        // {
        //     // 实际接入你的标志系统
        //     return GameFlags.IsSet(flagName) ? 1 : 0;
        // }
    }
}
