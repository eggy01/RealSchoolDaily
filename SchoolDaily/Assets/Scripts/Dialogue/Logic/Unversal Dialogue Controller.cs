using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;


namespace SchoolD.Dialogue
{
    //交互触发
    //[RequireComponent(typeof(NPCMovement))] npc移动代码

    // [RequireComponent(typeof(BoxCollider2D))]
    public class UnversalDialogueController : MonoBehaviour
    {
        private bool istalking; // 记录是否正在说话
        //public TextAsset[] csvFiles; // 剧情文件

        public List<DialoguePiece> dialogueList = new List<DialoguePiece>(); // 存储每一句对话的list
        private Stack<DialoguePiece> dialogueStack = new Stack<DialoguePiece>();

        private string CurrentcsvFileName;//当前剧情文件的名字

        public bool hasActiveDialogue = false;//是否存在激活剧情

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
        private void LoadNextDialogueByIndex(string indexStr)
        {
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
                var piece = dialogueStack.Peek(); // 先查看但不弹出

                // 条件检查
                if (!string.IsNullOrEmpty(piece.prerequisites) && !piece.prerequisites.Contains("|") && !piece.IsConditionsMet())
                {
                    Debug.Log($"对话终止，条件不满足: {piece.prerequisites}");
                    istalking = false;

                    yield break; // 直接跳出协程
                }

                // 只有条件满足时才继续
                piece = dialogueStack.Pop();
                EventHandler.CallShowDialogueEvent(piece);
                if (Input.GetKeyDown(KeyCode.P))
                    break;

                // 等待对话完成
                yield return new WaitUntil(() => piece.isDone);

                // 等待玩家输入继续
                if (piece.hasToPause)
                {
                    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
                }
            }

            // 对话结束
            EventHandler.CallShowDialogueEvent(null);

            istalking = false;
            dialogueList.Clear();

            StoryProgressManager.Instance.MarkStoryAsCompleted(CurrentcsvFileName);//标记该剧情已过

            hasActiveDialogue = false; // 重置标记，以便下次可以重新加载剧情文件

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
