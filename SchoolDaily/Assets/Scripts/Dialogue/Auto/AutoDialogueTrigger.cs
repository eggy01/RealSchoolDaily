using UnityEngine;

namespace SchoolD.Dialogue
{
    public abstract class AutoDialogueTrigger : MonoBehaviour
    {
        [Header("基础设置")]
        public string dialogueID;  // 对应DialogueManager注册的ID
        public bool triggerOnce = true;
        protected bool hasTriggered;

        protected virtual void TriggerDialogue()
        {
            if (!triggerOnce || !hasTriggered)
            {
                DialogueManager.Instance.TriggerDialogue(dialogueID);
                hasTriggered = true;
            }
        }
    }
}