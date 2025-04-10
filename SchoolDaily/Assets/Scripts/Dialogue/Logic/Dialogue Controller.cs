using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace SchoolD.Dialogue
{
    //交互触发
    //[RequireComponent(typeof(NPCMovement))] npc移动代码

    // [RequireComponent(typeof(BoxCollider2D))]
    public class DialogueController : MonoBehaviour
    {
        public UnityEvent OnFinishEvent; //完成后触发的事件

        private bool canTalk; // 判断npc是否处于可以对话状态, 可能为npc的一个属性
        public GameObject canTalkUI; // 可对话图标

        private bool istalking; // 记录是否正在说话
        public TextAsset[] csvFiles; // 剧情文件

        public List<DialoguePiece> dialogueList = new List<DialoguePiece>(); // 存储每一句对话的list
        private Stack<DialoguePiece> dialogueStack;

        private void Awake()
        {
            canTalkUI = transform.Find("CanTalkIcon").gameObject; // 假设可对话图标的子对象名称为"CanTalkIcon"
            canTalkUI.SetActive(false); // 默认不显示可对话图标

            // 加载CSV数据
            if (csvFiles.Length > 0)
            {
                bool hasActiveDialogue = false;
                for (int i = 0; i < csvFiles.Length; i++)
                {
                    if (!StoryProgressManager.Instance.IsStoryCompleted(int.Parse(csvFiles[i].name))) // 再结合玩家获得的属性
                    {
                        Debug.Log("aaaa");
                        Debug.Log(csvFiles[i].name);
                        dialogueList = DialogueCSVReader.Instance.LoadDialogueData(csvFiles[i]);
                        hasActiveDialogue = true;
                        break;
                    }
                }

                if (hasActiveDialogue)
                {
                    canTalk = true;
                    canTalkUI.SetActive(canTalk);
                    FillDialogueStack();
                }
                else
                {
                    canTalkUI.SetActive(false); // 没有可对话的剧情则关闭图标
                }
            }
        }

        void Update()
        {
            if (canTalk && Input.GetKeyDown(KeyCode.E) && !istalking)
            {
                StartCoroutine(DialogueRoutine());
            }
            else if (canTalk && Input.GetKeyDown(KeyCode.Space) && istalking)
            {
                EventHandler.TriggerNextDialogue();
            }
        }

        private void FillDialogueStack()
        {
            dialogueStack = new Stack<DialoguePiece>();
            foreach (var piece in dialogueList)
            {
                piece.isDone = false;
                dialogueStack.Push(piece);
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

