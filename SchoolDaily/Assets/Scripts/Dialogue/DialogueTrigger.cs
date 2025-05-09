using UnityEngine;
using System.Collections.Generic;

namespace SchoolD.Dialogue
{
    public class DialogueTrigger : MonoBehaviour
    {
        [System.Serializable]
        public class DialogueOption
        {
            public TextAsset dialogueCSV;
            public string prerequisiteCondition;
            public bool isOnce = true;
            public int SkipIndex = 0;
        }
        public GameObject CanTalkUI;

        [Header("基础设置")]
        public bool requireKeyPress = false;
        public List<DialogueOption> dialogueOptions = new List<DialogueOption>();

        private void Start()
        {
            // 移除ResetTriggerStates调用，只在需要时手动调用
            CheckAndRegisterDialogues();
        }

        private void CheckAndRegisterDialogues()
        {
            bool shouldDestroy = true;

            foreach (var option in dialogueOptions)
            {
                bool isCompleted = option.SkipIndex > 0
                    ? StoryProgressManager.Instance.IsDialogueLineCompleted(option.dialogueCSV.name, option.SkipIndex)
                    : StoryProgressManager.Instance.IsStoryCompleted(option.dialogueCSV.name);

                if (!isCompleted)
                {
                    // 只注册一次
                    if (option.SkipIndex > 0)
                    {
                        DialogueManager.Instance.RegisterDialogue(option.dialogueCSV, true, option.SkipIndex.ToString());
                    }
                    else
                    {
                        DialogueManager.Instance.RegisterDialogue(option.dialogueCSV);
                    }
                    shouldDestroy = false;
                }
            }

            if (shouldDestroy) Destroy(gameObject);
        }



        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && !requireKeyPress)
            {
                TryTriggerDialogue();
            }

        }

        private void TryTriggerDialogue()
        {
            foreach (var option in dialogueOptions)
            {
                if (ShouldSkipDialogue(option))
                {
                    continue;
                }

                if (CanTriggerDialogue(option))
                {
                    TriggerDialogue(option);
                    return;
                }
            }
        }

        private bool ShouldSkipDialogue(DialogueOption option)
        {
            // 如果是可重复对话，永不跳过
            if (!option.isOnce) return false;
            // 一次性对话检查完成状态
            return !StoryProgressManager.Instance.IsDialogueLineCompleted(option.dialogueCSV.name, option.SkipIndex) && StoryProgressManager.Instance.IsStoryCompleted(option.dialogueCSV.name);
        }

        private bool CanTriggerDialogue(DialogueOption option)
        {
            if (!string.IsNullOrEmpty(option.prerequisiteCondition))
            {
                bool conditionMet = ConditionSystem.CheckAll(option.prerequisiteCondition);
                if (!conditionMet) return false;
            }

            bool canUnlock = StoryProgressManager.Instance.CanUnlockStory(option.dialogueCSV.name);
            // 检查条件是否满足
            if (!string.IsNullOrEmpty(option.prerequisiteCondition) &&
                !ConditionSystem.CheckAll(option.prerequisiteCondition))
            {
                return false;
            }
            if (!StoryProgressManager.Instance.CanUnlockStory(option.dialogueCSV.name))
            {
                return false;
            }
            return true;
        }
        private void TriggerDialogue(DialogueOption option)
        {
            if (option.SkipIndex > 0)
            {
                EventHandler.CallLoadDialogueByIndex(option.SkipIndex.ToString(), option.dialogueCSV.name);
                // 正确标记完成
                if (!option.dialogueCSV.name.Equals("Tip0"))
                    StoryProgressManager.Instance.MarkDialogueLineCompleted(option.dialogueCSV.name, option.SkipIndex);
            }
            else
            {
                DialogueManager.Instance.TriggerDialogue(option.dialogueCSV.name);
            }

            CheckAllDialoguesCompleted();
        }

        private void CheckAllDialoguesCompleted()
        {
            foreach (var option in dialogueOptions)
            {
                if (!StoryProgressManager.Instance.IsDialogueLineCompleted(option.dialogueCSV.name, option.SkipIndex) && !StoryProgressManager.Instance.IsStoryCompleted(option.dialogueCSV.name))
                {
                    return;
                }
            }
            Destroy(gameObject);
        }

        // 编辑器可视化
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawCube(transform.position, GetComponent<Collider2D>().bounds.size);
        }

    }
}