using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;


namespace SchoolD.Dialogue
{
    public class UnversalDialogueController : MonoBehaviour
    {
        private bool istalking; // 记录是否正在说话
        //public TextAsset[] csvFiles; // 剧情文件

        public List<DialoguePiece> dialogueList = new List<DialoguePiece>(); // 存储每一句对话的list
        private Stack<DialoguePiece> dialogueStack = new Stack<DialoguePiece>();

        private string CurrentcsvFileName;//当前剧情文件的名字

        public bool hasActiveDialogue = false;//是否存在激活剧情
        public bool SkipIndex;

        private void Awake()
        { }
        private void OnNewDialogueStarted(List<DialoguePiece> newDialogueList, string newDialogueFileName)
        {
            dialogueList = newDialogueList;
            FillDialogueStack();

            // 2. 重置对话状态
            istalking = true;

            CurrentcsvFileName = newDialogueFileName;

            // 3. 启动新对话协程
            StartCoroutine(DialogueRoutine());
        }


        private void OnEnable()
        {
            EventHandler.OnStartNewDialogueEvent += OnNewDialogueStarted;
            EventHandler.OnLoadDialogueByIndex += LoadNextDialogueByIndex;
        }

        private void OnDisable()
        {
            EventHandler.OnStartNewDialogueEvent -= OnNewDialogueStarted;
            EventHandler.OnLoadDialogueByIndex -= LoadNextDialogueByIndex;
        }

        // 通过索引跳转
        private void LoadNextDialogueByIndex(string indexStr, string dialogueID)
        {
            SkipIndex = true;
            dialogueList = DialogueCSVReader.Instance.LoadDialogueData(DialogueCSVReader.LoadCSVFromResources(dialogueID));
            Debug.Log($"LoadNextDialogueByIndex被调用，indexStr:{indexStr} 当前对话列表长度:{dialogueList.Count}");
            if (int.TryParse(indexStr, out int targetIndex))
            {
                // 1. 找到所有匹配的对话片段
                var matchedPieces = dialogueList.FindAll(p => p.index == targetIndex);

                if (matchedPieces == null || matchedPieces.Count == 0)
                {
                    Debug.Log("333333333333 - 没有找到匹配的对话片段");
                    return;
                }
                Debug.Log("匹配对话：" + matchedPieces.Count);

                dialogueStack.Clear();
                for (int i = matchedPieces.Count - 1; i >= 0; i--)
                {
                    dialogueStack.Push(matchedPieces[i]);
                }

                // 6. 开始新对话
                istalking = true;
                StartCoroutine(DialogueRoutine());
            }
        }


        private void Update()
        {
            if (istalking && Input.GetKeyDown(KeyCode.Space))
            {
                EventHandler.TriggerNextDialogue();
            }
        }
        private IEnumerator DialogueRoutine()
        {
            istalking = true;

            while (dialogueStack.Count > 0)
            {
                var piece = dialogueStack.Pop(); // 直接弹出，避免Peek+Pop

                // // 条件检查
                // if (!string.IsNullOrEmpty(piece.prerequisites) &&
                //     !piece.prerequisites.Contains("|") &&
                //     !piece.IsConditionsMet())
                // {
                //     Debug.Log($"对话终止，条件不满足: {piece.prerequisites}");
                //     CleanUpDialogue();
                //     yield break;
                // }

                // 显示当前对话片段
                EventHandler.CallShowDialogueEvent(piece);

                // 等待对话完成
                yield return new WaitUntil(() => piece.isDone);

                // 等待玩家继续输入
                if (piece.hasToPause)
                {
                    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
                }
            }

            CleanUpDialogue();
        }

        private void CleanUpDialogue()
        {
            Debug.Log("清理：");
            EventHandler.CallShowDialogueEvent(null);
            istalking = false;
            dialogueList.Clear();
            dialogueStack.Clear();
            Debug.Log("Skip" + SkipIndex);
            if (!SkipIndex)
            {
                StoryProgressManager.Instance.MarkStoryAsCompleted(CurrentcsvFileName);
            }

            hasActiveDialogue = false;
            SkipIndex = false; // 重置标志
        }
        private void FillDialogueStack()
        {
            dialogueStack.Clear();
            for (int i = dialogueList.Count - 1; i > -1; i--)
            {
                dialogueList[i].isDone = false;
                dialogueStack.Push(dialogueList[i]);
            }
        }

    }
}
