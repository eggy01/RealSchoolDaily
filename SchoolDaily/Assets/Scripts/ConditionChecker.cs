using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionChecker
{
    public static bool CheckCondition(Condition condition)
    {
        int currentValue = GetCurrentValue(condition);

        return condition.comparison switch
        {
            CompareOperator.GreaterThan => currentValue > condition.requiredValue,
            CompareOperator.Equal => currentValue == condition.requiredValue,
            CompareOperator.LessThan => currentValue < condition.requiredValue,
            CompareOperator.GreaterOrEqual => currentValue >= condition.requiredValue,
            CompareOperator.LessOrEqual => currentValue <= condition.requiredValue,
            CompareOperator.NotEqual => currentValue != condition.requiredValue,
            _ => false
        };
    }

    private static int GetCurrentValue(Condition condition)
    {
        // 实际实现需要接入你的游戏系统
        // 这里用伪代码示例：
        switch (condition.type)
        {
            case ConditionType.Favorability:
                return SaveSystem.GetFavorability(condition.target);//获得目标好感度
            case ConditionType.ItemOwned:
            //return Inventory.GetItemCount(condition.target);
            // 其他类型处理...
            default:
                return 0;
        }
    }

    // // 批量检查（可选）
    // public static bool CheckAllConditions(Condition[] conditions)
    // {
    //     return conditions.All(CheckCondition);
    // }
}
