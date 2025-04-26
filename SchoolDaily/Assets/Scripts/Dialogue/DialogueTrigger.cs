using UnityEngine;
using System.Collections.Generic;
using System.Data.Common;

namespace SchoolD.Dialogue
{
    public class DialogueTrigger : MonoBehaviour
    {
        [System.Serializable]
        public class DialogueOption
        {
            //public string dialogueID;          // 唯一对话标识符
            public TextAsset dialogueCSV;      // 对话文件
            public string prerequisiteCondition; // 触发条件 例如"HasItem:StudentCard"
            public bool isOnce = true;         // 是否只触发一次
            public int SkipIndex = 0;
            [HideInInspector] public bool hasTriggered;
        }

        [Header("基础设置")]
        public bool requireKeyPress = false;    // 是否需要按键触发
        public List<DialogueOption> dialogueOptions = new List<DialogueOption>();

        private void Start()
        {
            int i = 0;
            // 自动注册所有对话
            foreach (var option in dialogueOptions)
            {
                if (StoryProgressManager.Instance.IsStoryCompleted(option.dialogueCSV.name))
                    i++;
                DialogueManager.Instance.RegisterDialogue(option.dialogueCSV);
            }
            if (i == dialogueOptions.Count)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (!requireKeyPress)
                {
                    TryTriggerDialogue();
                }
            }
        }
        public void Update()
        {
            int i = 0;
            // 自动注册所有对话
            foreach (var option in dialogueOptions)
            {
                if (StoryProgressManager.Instance.IsStoryCompleted(option.dialogueCSV.name))
                    i++;
            }
            if (i == dialogueOptions.Count)
                Destroy(gameObject);
        }

        // private void OnTriggerStay2D(Collider2D other)
        // {
        //     if (requireKeyPress && Input.GetKeyDown(KeyCode.E))
        //     {
        //         TryTriggerDialogue();
        //     }
        // }
        private void TryTriggerDialogue()
        {
            foreach (var option in dialogueOptions)
            {
                // 跳过已完成的单次对话
                if (option.isOnce && option.hasTriggered) continue;

                // 检查剧情是否已完成
                if (StoryProgressManager.Instance.IsStoryCompleted(option.dialogueCSV.name))
                {
                    Debug.Log($"剧情已完成: {option.dialogueCSV.name}");
                    continue;
                }

                // 检查触发条件
                if (!string.IsNullOrEmpty(option.prerequisiteCondition) &&
                    !ConditionSystem.Check(option.prerequisiteCondition))
                {
                    Debug.Log($"条件未满足: {option.prerequisiteCondition}");
                    continue;
                }
                if (option.SkipIndex > 0)
                {
                    EventHandler.CallLoadDialogueByIndex(option.SkipIndex.ToString(), option.dialogueCSV.name);
                }
                else
                {
                    // 触发符合条件的第一个对话
                    DialogueManager.Instance.TriggerDialogue(option.dialogueCSV.name);
                }


                option.hasTriggered = true;
                return;
            }

            Debug.Log("没有符合条件的对话可触发");
        }

        // 编辑器可视化
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawCube(transform.position, GetComponent<Collider2D>().bounds.size);
        }
    }
}