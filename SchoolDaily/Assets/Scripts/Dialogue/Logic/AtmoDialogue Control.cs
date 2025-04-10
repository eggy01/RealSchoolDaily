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
        private Stack<DialoguePiece> dialogueStack;

        //public TextAsset[] csvFiles;//剧情文件
        public TextAsset csvFile;

        public int storyId;//剧情序号
        private void Awake()
        {
            // 加载CSV数据
            // if (csvFiles.Length > 0)
            // {
            //     for (int i = 0; i < csvFiles.Length; i++)
            //     {
            //         if (!StoryProgressManager.Instance.IsStoryCompleted(int.Parse(csvFiles[i].name)))
            //         {
            //             storyId = i;
            //             dialogueList = DialogueCSVReader.Instance.LoadDialogueData(csvFiles[storyId]);
            //             break;
            //         }
            //     }
            // }

            dialogueList = DialogueCSVReader.Instance.LoadDialogueData(csvFile);
            FillDialogueStack();
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (!StoryProgressManager.Instance.IsStoryCompleted(storyId))
                {//如果该剧情没过
                    StartCoroutine(DialogueRoutine());
                }
                else
                {
                    Destroy(gameObject);//如果过了销毁该触发器
                }

            }
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.E) && !istalking)
            {
                StartCoroutine(DialogueRoutine());
            }
            else if (Input.GetKeyDown(KeyCode.Space) && istalking)
            {
                EventHandler.TriggerNextDialogue();
            }
        }
        private void FillDialogueStack()
        {
            dialogueStack = new Stack<DialoguePiece>();
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
            FillDialogueStack();
            istalking = false;
            OnFinishEvent?.Invoke();
        }

    }
}
