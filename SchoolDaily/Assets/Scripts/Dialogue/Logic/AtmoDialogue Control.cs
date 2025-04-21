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

        public TextAsset[] csvFiles; // 剧情文件
        private string CurrentcsvFileName;//当前剧情文件的名字

        private void Awake()
        {
            dialogueList = DialogueCSVReader.Instance.LoadDialogueData(csvFiles[0]);
            CurrentcsvFileName = csvFiles[0].name;
            FillDialogueStack();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                for (int i = 0; i < csvFiles.Length; i++)
                {
                    Debug.Log(csvFiles[i].name + "完成：" + StoryProgressManager.Instance.IsStoryCompleted(csvFiles[i].name));
                    Debug.Log(StoryProgressManager.Instance.CanUnlockStory(csvFiles[i].name));
                    if (StoryProgressManager.Instance.CanUnlockStory(csvFiles[i].name) && !StoryProgressManager.Instance.IsStoryCompleted(csvFiles[i].name)) // 再结合玩家获得的属性
                    {
                        dialogueList = DialogueCSVReader.Instance.LoadDialogueData(csvFiles[i]);
                        CurrentcsvFileName = csvFiles[i].name;
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(CurrentcsvFileName))
                {
                    FillDialogueStack();
                    StartCoroutine(DialogueRoutine());
                }
                else
                {
                    Debug.Log("删除触发器");
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
            CurrentcsvFileName = "";

            StoryProgressManager.Instance.MarkStoryAsCompleted(CurrentcsvFileName);

            OnFinishEvent?.Invoke();
        }

    }
}
