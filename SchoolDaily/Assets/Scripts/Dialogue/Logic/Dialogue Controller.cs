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
    public class DialogueController : MonoBehaviour
    {
        public UnityEvent OnFinishEvent; //完成后触发的事件

        private bool canTalk; // 判断npc是否处于可以对话状态, 可能为npc的一个属性
        public GameObject canTalkUI; // 可对话图标

        private bool istalking; // 记录是否正在说话

        public bool playerInRange; // 跟踪玩家是否在范围内
        public TextAsset[] csvFiles; // 剧情文件

        public List<DialoguePiece> dialogueList = new List<DialoguePiece>(); // 存储每一句对话的list
        private Stack<DialoguePiece> dialogueStack = new Stack<DialoguePiece>();

        private string CurrentcsvFileName;//当前剧情文件的名字

        public bool hasActiveDialogue = false;//是否存在激活剧情

        private void Awake()
        {
            canTalkUI = transform.Find("CanTalkIcon").gameObject; // 假设可对话图标的子对象名称为"CanTalkIcon"
            canTalkUI.SetActive(false); // 默认不显示可对话图标
        }

        private void Start()
        {
            if (csvFiles.Length < 1)
                Debug.LogError("必须有一个默认对话文件");
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
            // 如果已经加载过剧情文件，则不再加载
            if (hasActiveDialogue)
            {
                return;
            }
            // 加载CSV数据
            if (csvFiles.Length > 0)
            {
<<<<<<< Updated upstream
                Debug.Log(StoryProgressManager1.Instance.IsStoryCompleted("Beginner_02"));
                Debug.Log(StoryProgressManager1.Instance.CanUnlockStory("Beginner_03"));
                for (int i = 0; i < csvFiles.Length; i++)
                {
                    //!StoryProgressManager.Instance.IsStoryCompleted(int.Parse(csvFiles[i].name))
                    if (StoryProgressManager1.Instance.CanUnlockStory(csvFiles[i].name) && !StoryProgressManager1.Instance.IsStoryCompleted(csvFiles[i].name)) // 再结合玩家获得的属性
=======
                for (int i = 0; i < csvFiles.Length; i++)
                {
                    if (StoryProgressManager.Instance.CanUnlockStory(csvFiles[i].name) && !StoryProgressManager.Instance.IsStoryCompleted(csvFiles[i].name)) // 再结合玩家获得的属性
>>>>>>> Stashed changes
                    {
                        dialogueList = DialogueCSVReader.Instance.LoadDialogueData(csvFiles[i]);
                        hasActiveDialogue = true;
                        CurrentcsvFileName = csvFiles[i].name;
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
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
            }
        }

        private void Update()
        {
            if (!playerInRange) return;

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (canTalk && !istalking)
                {
                    StartCoroutine(DialogueRoutine());
                }
            }

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
            dialogueList.Clear();

            StoryProgressManager1.Instance.MarkStoryAsCompleted(CurrentcsvFileName);//标记该剧情已过
            canTalkUI.SetActive(false);

            hasActiveDialogue = false; // 重置标记，以便下次可以重新加载剧情文件


            OnFinishEvent?.Invoke();
        }

        private void OnMouseOver()
        {
            if (Input.GetMouseButtonDown(1)) // 1表示右键
            {
                if (playerInRange && !hasActiveDialogue)
                {
                    TriggerDefaultDialogue();
                }
            }
        }

        private void TriggerDefaultDialogue()
        {
            // 加载默认对话
            dialogueList = DialogueCSVReader.Instance.LoadDialogueData(csvFiles[0]);
            hasActiveDialogue = true;
            CurrentcsvFileName = csvFiles[0].name;

            FillDialogueStack();// 准备对话

            // 如果不在对话中，则开始对话
            if (!istalking)
            {
                StartCoroutine(DialogueRoutine());
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

    }
}
