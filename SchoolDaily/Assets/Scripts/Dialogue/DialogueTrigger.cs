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

            // 改进的持久化方案
            public bool HasTriggered
            {
                get
                {
                    // 如果剧情已完成，则视为已触发
                    if (StoryProgressManager.Instance.IsStoryCompleted(dialogueCSV.name))
                        return true;

                    return PlayerPrefs.GetInt($"DialogueTriggered_{dialogueCSV.name}", 0) == 1;
                }
                set
                {
                    PlayerPrefs.SetInt($"DialogueTriggered_{dialogueCSV.name}", value ? 1 : 0);
                    PlayerPrefs.Save(); // 确保立即保存
                }
            }
        }

        [Header("基础设置")]
        public bool requireKeyPress = false;
        public List<DialogueOption> dialogueOptions = new List<DialogueOption>();
        private void Start()
        {
            CheckAndRegisterDialogues();
            ResetTriggerStates();
        }

        private void CheckAndRegisterDialogues()
        {
            bool allCompleted = true;

            foreach (var option in dialogueOptions)
            {
                bool isCompleted = StoryProgressManager.Instance.IsStoryCompleted(option.dialogueCSV.name);
                if (!isCompleted)
                {
                    allCompleted = false;
                    DialogueManager.Instance.RegisterDialogue(option.dialogueCSV);
                }
            }

            if (allCompleted)
            {
                Destroy(gameObject);
            }
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

            Debug.Log("没有符合条件的对话可触发");
        }

        private bool ShouldSkipDialogue(DialogueOption option)
        {
            // 如果是一次性对话且已触发，则跳过
            return option.isOnce && option.HasTriggered;
        }

        private bool CanTriggerDialogue(DialogueOption option)
        {
            // 检查条件是否满足
            if (!string.IsNullOrEmpty(option.prerequisiteCondition) &&
                !ConditionSystem.Check(option.prerequisiteCondition))
            {
                Debug.Log($"条件未满足或未解锁: {option.prerequisiteCondition}");
                return false;
            }
            if (!StoryProgressManager.Instance.CanUnlockStory(option.dialogueCSV.name))
            {
                Debug.Log($"未解锁: ");
                return false;
            }
            return true;
        }

        private void TriggerDialogue(DialogueOption option)
        {
            if (option.SkipIndex > 0)
            {
                EventHandler.CallLoadDialogueByIndex(option.SkipIndex.ToString(), option.dialogueCSV.name);
            }
            else
            {
                DialogueManager.Instance.TriggerDialogue(option.dialogueCSV.name);
            }

            // 标记为已触发
            option.HasTriggered = true;

            // 立即检查是否所有对话都完成了
            CheckAllDialoguesCompleted();
        }

        private void CheckAllDialoguesCompleted()
        {
            foreach (var option in dialogueOptions)
            {
                if (!option.HasTriggered && !StoryProgressManager.Instance.IsStoryCompleted(option.dialogueCSV.name))
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

        // 只在需要时手动调用重置
        public void ResetTriggerStates()
        {
            foreach (var option in dialogueOptions)
            {
                PlayerPrefs.DeleteKey($"DialogueTriggered_{option.dialogueCSV.name}");
            }
            PlayerPrefs.Save();
        }
    }
}