using UnityEngine;
using SchoolD.Dialogue;

public class MultiConditionTrigger : AutoDialogueTrigger
{
    public string conditions;

    void Update()
    {
        if (ConditionSystem.CheckAll(conditions))
        {
            Debug.Log(dialogueID);
            DialogueManager.Instance.TriggerDialogue(dialogueID);
            Destroy(this.gameObject); // 触发后销毁
        }
    }
}
