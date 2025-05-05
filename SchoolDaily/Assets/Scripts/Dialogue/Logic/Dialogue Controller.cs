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

        private bool playerInRange; // 跟踪玩家是否在范围内
        public TextAsset DefaultcsvFile;
        public TextAsset[] csvFiles; // 剧情文件

        private List<DialoguePiece> dialogueList = new List<DialoguePiece>(); // 存储每一句对话的list
        private Stack<DialoguePiece> dialogueStack = new Stack<DialoguePiece>();

        private string CurrentcsvFileName;//当前剧情文件的名字

        private bool hasActiveDialogue = false;//是否存在激活剧情
        //[Header("跳过设置")]
        public Button skipButton;
        private bool isDialogueSkipping = false;

        private void Awake()
        {
            canTalkUI = transform.Find("CanTalkIcon").gameObject; // 假设可对话图标的子对象名称为"CanTalkIcon"
            canTalkUI.SetActive(false); // 默认不显示可对话图标

            // 自动注册所有对话
            foreach (var csv in csvFiles)
            {
                DialogueManager.Instance.RegisterDialogue(csv);
            }
            if (DefaultcsvFile != null)
                DialogueManager.Instance.RegisterDialogue(DefaultcsvFile);
        }
        private void Start()
        {
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
                for (int i = 0; i < csvFiles.Length; i++)
                {
                    Debug.Log("能被解锁：" + csvFiles[i].name + StoryProgressManager.Instance.CanUnlockStory(csvFiles[i].name));
                    Debug.Log("完成状态" + csvFiles[i].name + StoryProgressManager.Instance.IsStoryCompleted(csvFiles[i].name));
                    if (!StoryProgressManager.Instance.IsStoryCompleted(csvFiles[i].name) && StoryProgressManager.Instance.CanUnlockStory(csvFiles[i].name)) // 再结合玩家获得的属性
                    {
                        dialogueList = DialogueCSVReader.Instance.LoadDialogueData(csvFiles[i]);
                        if (dialogueList != null)
                            if (string.IsNullOrEmpty(dialogueList[0].prerequisites))
                            {
                                hasActiveDialogue = true;
                                CurrentcsvFileName = csvFiles[i].name;
                                Debug.Log("当前剧情：" + CurrentcsvFileName);
                                break;
                            }
                            else
                        if (ConditionSystem.CheckAll(dialogueList[0].prerequisites))
                            {
                                hasActiveDialogue = true;
                                CurrentcsvFileName = csvFiles[i].name;
                                Debug.Log("当前剧情：" + CurrentcsvFileName);
                                break;
                            }
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
                else if (!hasActiveDialogue)
                    TriggerDefaultDialogue();
            }

            if (istalking && Input.GetKeyDown(KeyCode.Space))
            {
                EventHandler.TriggerNextDialogue();
            }
        }
        private IEnumerator DialogueRoutine()
        {
            try
            {
                istalking = true;

                while (dialogueStack.Count > 0 && !isDialogueSkipping)
                {
                    var piece = dialogueStack.Peek(); // 先查看但不弹出

                    // 条件检查
                    if (!string.IsNullOrEmpty(piece.prerequisites) && !piece.prerequisites.Contains("|") && !piece.IsConditionsMet())
                    {
                        Debug.Log($"对话终止，条件不满足: {piece.prerequisites}");
                        canTalkUI.SetActive(false);
                        istalking = false;

                        yield break; // 直接跳出协程
                    }

                    // 只有条件满足时才继续
                    piece = dialogueStack.Pop();
                    EventHandler.CallShowDialogueEvent(piece);

                    // // 检查是否需要跳转
                    // if (!string.IsNullOrEmpty(piece.nextIndex))
                    // {
                    //     Debug.Log($"跳转到新对话: {piece.nextIndex}");
                    //     istalking = false;
                    //     canTalkUI.SetActive(false);
                    //     hasActiveDialogue = false;
                    //     //EventHandler.CallLoadDialogueByIndex(piece.nextIndex, piece.belongToCSVFileName);
                    //     yield break; // 终止当前协程
                    // }
                    //等待对话完成
                    yield return new WaitUntil(() => piece.isDone);



                    // 等待玩家输入继续
                    if (piece.hasToPause)
                    {
                        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
                    }
                }
            }
            finally
            {
                CleanUpDialogue();
            }


        }

        private void CleanUpDialogue()
        {
            // 对话结束
            EventHandler.CallShowDialogueEvent(null);
            istalking = false;
            dialogueList.Clear();
            if (!CurrentcsvFileName.Contains("Default"))
                StoryProgressManager.Instance.MarkStoryAsCompleted(CurrentcsvFileName);

            canTalkUI.SetActive(false);
            hasActiveDialogue = false;

            OnFinishEvent?.Invoke();
        }

        private void TriggerDefaultDialogue()
        {
            if (DefaultcsvFile == null)
                return;

            dialogueList = DialogueCSVReader.Instance.LoadDialogueData(DefaultcsvFile);

            hasActiveDialogue = true;
            CurrentcsvFileName = DefaultcsvFile.name;

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
