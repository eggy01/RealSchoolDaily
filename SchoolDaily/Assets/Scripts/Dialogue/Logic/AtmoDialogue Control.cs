using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SchoolD.Dialogue
{
    //自动触发和区域触发
    public class AtmoDialogueControl : MonoBehaviour
    {
        public UnityEvent OnFinishEvent;//完成后触发的事件，
        private bool istalking;//记录是否正在说话
        public List<DialoguePiece> dialogueList = new List<DialoguePiece>();//存储每一句对话的list
        private Stack<DialoguePiece> dialogueStack = new Stack<DialoguePiece>();

        public TextAsset csvFile;

        private string CurrentcsvFileName;//当前剧情文件的名字

        private void OnEnable()
        {
            // 订阅事件
            EventHandler.OnStartNewDialogueEvent += OnNewDialogueStarted;
        }

        private void OnDisable()
        {
            // 取消订阅（防止内存泄漏）
            EventHandler.OnStartNewDialogueEvent -= OnNewDialogueStarted;
        }

        // 事件处理逻辑
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
        private void Awake()
        {
            dialogueList = DialogueCSVReader.Instance.LoadDialogueData(csvFile);
            CurrentcsvFileName = csvFile.name;
            FillDialogueStack();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (!StoryProgressManager1.Instance.IsStoryCompleted(CurrentcsvFileName))
                {//如果该剧情没过
                    StartCoroutine(DialogueRoutine());
                }
                else
                {
                    Destroy(gameObject);//如果过了销毁该触发器
                }

            }
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
        private IEnumerator DialogueRoutine()
        {
            istalking = true;

            while (dialogueStack.Count > 0)
            {
                var piece = dialogueStack.Pop();
                EventHandler.CallShowDialogueEvent(piece);

                // 等待对话完成
                yield return new WaitUntil(() => piece.isDone);

                // 等待玩家输入继续
                if (piece.hasToPause)
                {
                    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0));
                }
            }

            // 对话结束
            EventHandler.CallShowDialogueEvent(null);

            istalking = false;

            // StoryProgressManager.Instance.MarkStoryAsCompleted(int.Parse(csvFile.name));//标记该剧情已过
            StoryProgressManager1.Instance.MarkStoryAsCompleted(CurrentcsvFileName);

            OnFinishEvent?.Invoke();
        }

    }
}
