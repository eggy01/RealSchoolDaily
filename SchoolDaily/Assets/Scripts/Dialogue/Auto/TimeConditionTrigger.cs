using System.Collections;
using System.Collections.Generic;
using SchoolD.Dialogue;
using UnityEngine;

public class TimeConditionTrigger : AutoDialogueTrigger
{
    [Header("时间条件")]
    public string timeCondition; // 例如："Time>18:00"

    private void Update()
    {
        if (ConditionSystem.Check(timeCondition))
        {
            TriggerDialogue();
            enabled = false; // 触发后禁用
        }
    }
}
