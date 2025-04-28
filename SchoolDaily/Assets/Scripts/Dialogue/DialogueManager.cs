using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace SchoolD.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [Header("全局设置")]
        public float dialogueCooldown = 0.5f;

        private Dictionary<string, DialogueInfo> registeredDialogue = new Dictionary<string, DialogueInfo>();
        private float lastDialogueTime;
        private string currentDialogueID;

        // 新增：封装对话信息和标记设置
        private class DialogueInfo
        {
            public TextAsset csvFile;
            public bool shouldMarkComplete;
        }

        private void Awake()
        {
            Instance = this;
        }

        // 修改后的注册方法（增加shouldMarkComplete参数）
        // // 常规对话（完成后会标记）
        // DialogueManager.Instance.RegisterDialogue(normalDialogueCSV);

        // // 特殊对话（不标记完成）
        // DialogueManager.Instance.RegisterDialogue(
        //     specialDialogueCSV, 
        //     shouldMarkComplete: false,
        //     dialogueID: "RepeatableDialogue"
        // );

        public void RegisterDialogue(TextAsset csvFile, bool shouldMarkComplete = true, string dialogueID = "")
        {
            string id = string.IsNullOrEmpty(dialogueID) ? csvFile.name : dialogueID;

            if (!registeredDialogue.ContainsKey(id))
            {
                registeredDialogue.Add(id, new DialogueInfo
                {
                    csvFile = csvFile,
                    shouldMarkComplete = shouldMarkComplete
                });
                Debug.Log($"注册对话: {id} ({(shouldMarkComplete ? "可标记完成" : "不标记完成")})");
            }
        }

        // 修改后的自动注册方法
        public void RegisterAutoTrigger(string id, TextAsset csv, bool shouldMarkComplete = true)
        {
            RegisterDialogue(csv, shouldMarkComplete, id);
        }

        public void TriggerDialogue(string dialogueID)
        {
            if (Time.time < lastDialogueTime + dialogueCooldown) return;
            if (dialogueID == currentDialogueID) return;

            if (registeredDialogue.TryGetValue(dialogueID, out DialogueInfo info))
            {
                StartCoroutine(DialogueRoutine(info.csvFile, dialogueID, info.shouldMarkComplete));
                lastDialogueTime = Time.time;
                currentDialogueID = dialogueID;
            }
            else
            {
                Debug.LogWarning($"未注册的对话ID: {dialogueID}");
            }
        }

        // 修改后的协程（增加shouldMark参数）
        public IEnumerator DialogueRoutine(TextAsset csvFile, string dialogueID, bool shouldMark)
        {
            var dialogueList = DialogueCSVReader.Instance.LoadDialogueData(csvFile);
            var stack = new Stack<DialoguePiece>();

            for (int i = dialogueList.Count - 1; i >= 0; i--)
            {
                dialogueList[i].isDone = false;
                stack.Push(dialogueList[i]);
            }

            while (stack.Count > 0)
            {
                var piece = stack.Pop();
                EventHandler.CallShowDialogueEvent(piece);
                yield return new WaitUntil(() => piece.isDone);

                if (piece.hasToPause)
                    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
            }

            EventHandler.CallShowDialogueEvent(null);

            // 根据参数决定是否标记完成
            if (shouldMark)
            {
                StoryProgressManager.Instance.MarkStoryAsCompleted(csvFile.name);
            }
            else
            {
                Debug.Log($"对话完成但未标记: {dialogueID}");
            }

            currentDialogueID = null;
        }
    }
}