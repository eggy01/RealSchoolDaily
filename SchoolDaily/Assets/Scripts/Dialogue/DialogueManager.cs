using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace SchoolD.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [Header("全局设置")]
        public float dialogueCooldown = 0.5f; // 对话冷却时间

        private Dictionary<string, TextAsset> registeredDialogue = new Dictionary<string, TextAsset>();
        private float lastDialogueTime;
        private string currentDialogueID;

        private void Awake()
        {
            Instance = this;
        }

        // 注册剧情文件（可由触发器调用）
        public void RegisterDialogue(TextAsset csvFile)
        {
            string dialogueID = csvFile.name;
            if (!registeredDialogue.ContainsKey(dialogueID))
            {
                registeredDialogue.Add(dialogueID, csvFile);
                Debug.Log($"注册对话: {dialogueID}");
            }
        }

        // 触发对话（外部调用）
        public void TriggerDialogue(string dialogueID)
        {
            // 冷却检查
            if (Time.time < lastDialogueTime + dialogueCooldown) return;

            // 重复对话检查
            if (dialogueID == currentDialogueID) return;

            if (registeredDialogue.TryGetValue(dialogueID, out TextAsset csv))
            {
                StartCoroutine(DialogueRoutine(csv, dialogueID));
                lastDialogueTime = Time.time;
                currentDialogueID = dialogueID;
            }
            else
            {
                Debug.LogWarning($"未注册的对话ID: {dialogueID}");
            }
        }

        public IEnumerator DialogueRoutine(TextAsset csvFile, string dialogueID)
        {
            var dialogueList = DialogueCSVReader.Instance.LoadDialogueData(csvFile);
            var stack = new Stack<DialoguePiece>();

            // 填充对话栈
            for (int i = dialogueList.Count - 1; i >= 0; i--)
            {
                dialogueList[i].isDone = false;
                stack.Push(dialogueList[i]);
            }

            // 执行对话
            while (stack.Count > 0)
            {
                var piece = stack.Pop();
                EventHandler.CallShowDialogueEvent(piece);
                yield return new WaitUntil(() => piece.isDone);

                if (piece.hasToPause)
                    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
            }

            // 对话结束
            EventHandler.CallShowDialogueEvent(null);
            StoryProgressManager.Instance.MarkStoryAsCompleted(dialogueID);
            currentDialogueID = null;
        }
    }
}
