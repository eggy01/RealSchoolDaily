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

        private int SortID;//剧情文件的名字

        private void Awake()
        {
            canTalkUI = transform.Find("CanTalkIcon").gameObject; // 假设可对话图标的子对象名称为"CanTalkIcon"
            canTalkUI.SetActive(false); // 默认不显示可对话图标
        }

        private void Start()
        {
            // 初始化后立即检测一次
            CheckAvailableDialogue();

            // 然后开始定期检测
            StartCoroutine(PeriodicCheck());
        }

        private IEnumerator PeriodicCheck()
        {
            while (true)
            {
                yield return new WaitForSeconds(Settings.checkInterval);
                if (!istalking)
                {
                    CheckAvailableDialogue();
                }
            }
        }
        private void CheckAvailableDialogue()
        {
            // 加载CSV数据
            if (csvFiles.Length > 0)
            {
                bool hasActiveDialogue = false;
                for (int i = 0; i < csvFiles.Length; i++)
                {
                    if (!StoryProgressManager.Instance.IsStoryCompleted(int.Parse(csvFiles[i].name))) // 再结合玩家获得的属性
                    {
                        dialogueList = DialogueCSVReader.Instance.LoadDialogueData(csvFiles[i]);
                        hasActiveDialogue = true;
                        SortID = int.Parse((csvFiles[i].name));
                        Debug.Log("当前加载的文件名：" + csvFiles[i].name);
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

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (Input.GetKeyDown(KeyCode.E))
                    Debug.Log("按下e");
                if (canTalk && Input.GetKeyDown(KeyCode.E) && !istalking)
                {
                    StartCoroutine(DialogueRoutine());
                }
                else if (istalking && Input.GetKeyDown(KeyCode.Space))
                {
                    EventHandler.TriggerNextDialogue();
                }
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
            Debug.Log("对话协程启动，剩余对话数: " + dialogueStack.Count);

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
            dialogueList.Clear();

            StoryProgressManager.Instance.MarkStoryAsCompleted(SortID);//标记该剧情已过
            canTalkUI.SetActive(false);


            OnFinishEvent?.Invoke();
        }
    }
}
