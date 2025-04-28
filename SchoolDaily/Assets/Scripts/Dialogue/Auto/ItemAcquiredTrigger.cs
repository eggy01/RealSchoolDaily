using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SchoolD.Dialogue;
public class ItemAcquiredTrigger : AutoDialogueTrigger
{
    [Header("道具条件")]
    public string requiredItemID; // 需要获得的道具ID

    private void OnEnable()
    {
        EventHandler.OnItemAdded += CheckItem;
    }

    private void OnDisable()
    {
        EventHandler.OnItemAdded -= CheckItem;
    }

    private void CheckItem(ItemData item)
    {
        if (item.ID == requiredItemID)
        {
            TriggerDialogue();
        }
    }
}

