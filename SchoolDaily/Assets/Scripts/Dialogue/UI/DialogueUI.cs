using System.Collections;
using System.Collections.Generic;
using SchoolD.Dialogue;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SchoolD.Task;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    // public static bool IsDialogueActive { get; private set; } // 全局对话状态标志
    public GameObject dialogueBoxTop;
    public GameObject dialogueBoxBottom;
    public TextMeshProUGUI dialogueText;//对话框文本
    public Image faceRight, faceLeft;//对话框人物图片
    public Image emotionRightImage, emotionLeftImage;//表情图片
    public TextMeshProUGUI nameRight, nameLeft;//对话人物名称
    public Button continueButton;//继续按钮的Button组件

    public GameObject CanTalkUI;

    public Sprite leftDialogSprite;
    public Sprite rightDialogSprite;
    private DialoguePiece currentPiece;
    private bool isAnimating = false;

    public GameObject optionsPanel; // 存放选项按钮的面板
    public Button optionButtonPrefab; // 选项按钮预制体

    private int selectedOptionIndex = -1; // 记录玩家选择的选项索引

    public Animator optionMove;

    public int nexePieceIndex = -1;
    private string MoveToPosition;

    public GameObject textbook;

    private void Awake()
    {
        Instance = this;
        dialogueText.text = "";
        continueButton.onClick.AddListener(ContinueDialogue);
        nameRight.text = "主角";//主角名字
        nameLeft.text = "...";
    }

    private void OnEnable()
    {
        EventHandler.ShowDialogueEvent += OnShowDialogueEvent;
    }

    private void OnDisable()
    {
        EventHandler.ShowDialogueEvent -= OnShowDialogueEvent;
        continueButton.onClick.RemoveListener(ContinueDialogue);
    }

    private void OnShowDialogueEvent(DialoguePiece piece)
    {
        currentPiece = piece;
        StartCoroutine(ShowDialogue(piece));
    }

    private void Update()
    {
        // 检测空格键按下且当前有对话在显示
        if (Input.GetKeyDown(KeyCode.Space) && currentPiece != null)
        {
            ContinueDialogue();
        }
    }

    private IEnumerator ShowDialogue(DialoguePiece piece)
    {

        PlayerController.Instance.movement.SetPause(true);
        if (piece != null)
        {
            if (piece.index == -1)
            {
                piece.isDone = true;
                SetAllFalse();
                yield break;
            }

            // 检查是否是手机屏幕消息
            if (!string.IsNullOrEmpty(piece.Loaction) && piece.Loaction.Contains("手机屏幕"))
            {
                Debug.Log("手机屏幕");
                // 显示手机聊天弹窗
                yield return ChatSystem.Instance.ShowPhoneMessage(piece);

                piece.isDone = true;
                continueButton.gameObject.SetActive(piece.hasToPause && piece.isDone);
                yield break;
            }

            if (string.IsNullOrEmpty(piece.name) && piece.effects != null && piece.effects.Count > 0)
            {
                {//处理效果：黑屏，场景跳转，时间跳转
                    yield return DialogueEffectExecutor.Instance.ExecuteEffects(piece.effects);
                }
            }
            else
            {
                piece.hasToPause = true;
                piece.isDone = false;
                dialogueText.text = "";
                emotionLeftImage.gameObject.SetActive(false);
                emotionRightImage.gameObject.SetActive(false);
                MoveToPosition = "";

                // 默认隐藏所有UI
                dialogueBoxTop.SetActive(false);
                dialogueBoxBottom.SetActive(false);
                nameLeft.gameObject.SetActive(false);
                nameRight.gameObject.SetActive(false);
                faceLeft.gameObject.SetActive(false);
                faceRight.gameObject.SetActive(false);
                dialogueText.gameObject.SetActive(false);
                continueButton.gameObject.SetActive(false);


                // 旁白模式：只显示 dialogueBoxTop 和 dialogueText
                if (piece.name.Equals("旁白") || piece.name.Equals("教程"))
                {
                    dialogueBoxBottom.SetActive(true);
                    dialogueText.gameObject.SetActive(true);
                }
                else if (piece.index == 0)//单人旁白
                {
                    dialogueBoxBottom.SetActive(true);
                    dialogueText.gameObject.SetActive(true);
                    faceLeft.gameObject.SetActive(true);
                    faceLeft.sprite = piece.faceImage;
                    faceLeft.SetNativeSize();
                }
                // 非旁白模式：正常显示对话
                else
                {
                    dialogueBoxTop.SetActive(true);
                    dialogueText.gameObject.SetActive(true);

                    if (!piece.name.Equals(string.Empty))
                    {
                        dialogueBoxBottom.SetActive(true);
                        nameLeft.gameObject.SetActive(true);
                        nameRight.gameObject.SetActive(true);
                        faceLeft.gameObject.SetActive(true);
                        faceRight.gameObject.SetActive(true);

                        // 处理角色名称、头像、表情等逻辑
                        Sprite emotionSprite = null;
                        if (piece.emotion != null && !piece.emotion.Equals(string.Empty))
                        {
                            emotionSprite = npcExpressionOffset.Instance.LoadEmotionSprite(piece.emotion, piece.name);
                        }

                        if (piece.name.Equals(Settings.playerName))//主角
                        {
                            if (piece.isfinalNotFirst == 0)//当主角第一个说话时
                            {
                                faceLeft.sprite = null;
                                emotionLeftImage.sprite = null;
                                nameLeft.text = "";
                            }
                            nameRight.text = piece.name;
                            if (emotionSprite != null)
                            {
                                emotionRightImage.sprite = emotionSprite;
                                emotionRightImage.SetNativeSize();
                                emotionRightImage.gameObject.SetActive(true);
                            }
                        }
                        else
                        {
                            string npcID = NPCLoad.Instance.GetNPCIDByName(piece.name);
                            if (!string.IsNullOrEmpty(npcID))
                                NPCManager.Instance.MeetNPC(npcID);
                            nameLeft.text = piece.name;
                            faceLeft.sprite = piece.faceImage;

                            if (faceLeft.sprite != null && !faceLeft.sprite.name.Equals("默认2"))
                                faceLeft.SetNativeSize();

                            if (emotionSprite != null)
                            {
                                npcExpressionOffset.Instance.UpdateExpression(emotionLeftImage, piece.name, emotionSprite);
                                emotionLeftImage.gameObject.SetActive(true);
                            }
                        }

                        if (faceLeft.sprite == null)
                        {
                            faceLeft.gameObject.SetActive(false);
                            emotionLeftImage.gameObject.SetActive(false);
                        }

                        // 设置对话框样式（左右对话）
                        if (piece.onLeft)
                        {
                            dialogueBoxTop.GetComponent<Image>().sprite = leftDialogSprite;
                            dialogueBoxBottom.GetComponent<Image>().sprite = rightDialogSprite;
                            SetImageColor(true, Settings.DialogueInactiveColor); // 左边亮
                        }
                        else
                        {
                            dialogueBoxTop.GetComponent<Image>().sprite = rightDialogSprite;
                            dialogueBoxBottom.GetComponent<Image>().sprite = leftDialogSprite;
                            SetImageColor(false, Settings.DialogueInactiveColor); // 右边亮
                        }
                    }
                }
                if (piece.name.Equals("教程"))
                {
                    yield return StartCoroutine(AnimateText(piece.dialogueText, 0.01f));
                    // 开始教程
                    TutorialSystem.Instance.StartInventoryTutorial(piece.dialogueText);

                    // 等待教程完成
                    yield return TutorialSystem.Instance.WaitForTutorialComplete();
                }
                else
                {
                    if (!string.IsNullOrEmpty(piece.dialogueText))
                        yield return StartCoroutine(AnimateText(piece.dialogueText, 0.01f));
                }



                if (piece.effects != null && piece.effects.Count > 0)
                {//处理效果：黑屏，场景跳转，时间跳转
                    Debug.Log("触发效果：" + piece.effects);
                    yield return DialogueEffectExecutor.Instance.ExecuteEffects(piece.effects);
                }
                // 处理选项或逐字动画
                if (piece.option != null && piece.option.Count > 0)
                {

                    List<Button> optionButtons = new List<Button>();

                    // 检查是否有前置条件
                    bool hasPreconditions = !string.IsNullOrEmpty(piece.prerequisites);
                    string[] preconditions = hasPreconditions ? piece.prerequisites.Split('|') : new string[0];
                    //Debug.Log($"所有选项内容: {string.Join("|", piece.option.Select(opt => $"'{opt}'"))}");
                    for (int i = 0; i < piece.option.Count; i++)
                    {
                        bool shouldShow = true;

                        // 只有存在前置条件时才检查
                        if (hasPreconditions && i < preconditions.Length && !string.IsNullOrEmpty(preconditions[i]))
                        {
                            Debug.Log(preconditions[i]);
                            shouldShow = ConditionSystem.Check(preconditions[i]);
                        }

                        if (shouldShow)
                        {
                            Button optionButton = Instantiate(optionButtonPrefab, optionsPanel.transform);
                            // 设置图像为原始尺寸
                            optionButton.image.SetNativeSize();
                            optionButton.image.type = Image.Type.Sliced;
                            // 获取 RectTransform
                            RectTransform rectTransform = optionButton.image.GetComponent<RectTransform>();
                            // 计算新高度（增加10%）
                            float newHeight = rectTransform.rect.height * 1.1f;
                            // 使用 SetSizeWithCurrentAnchors 调整高度，保持宽度不变
                            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rectTransform.rect.width);
                            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);

                            optionButton.GetComponentInChildren<TextMeshProUGUI>().text = piece.option[i];
                            int currentOptionIndex = i;
                            optionButton.onClick.AddListener(() => OnOptionSelected(currentOptionIndex));
                            optionButtons.Add(optionButton);
                        }
                    }

                    // 如果没有可显示的选项，自动继续
                    if (optionButtons.Count == 0)
                    {
                        selectedOptionIndex = 0;
                    }
                    else
                    {
                        optionMove.SetBool("existoption", true);
                        optionMove.SetBool("selected", false);

                        while (selectedOptionIndex == -1)
                        {
                            yield return null;
                        }

                        optionMove.SetBool("selected", true);
                        optionMove.SetBool("existoption", false);
                        yield return new WaitForSeconds(0.5f);
                    }

                    foreach (Button button in optionButtons)
                    {
                        Destroy(button.gameObject);
                    }

                    if (!ProcessOption(selectedOptionIndex, piece.option))
                        yield break;
                    selectedOptionIndex = -1;
                }

                //处理任务
                if (!string.IsNullOrEmpty(piece.task))
                {
                    if (piece.task.Contains("接受任务"))
                    {
                        string pid = piece.task.Replace("接受任务:", "").Trim();
                        TaskSystem.Instance.StartTask(pid);

                    }
                    else if (piece.task.Contains("完成任务:"))
                    {
                        string pid = piece.task.Replace("完成任务:", "").Trim();
                        TaskSystem.Instance.CompleteTask(pid);
                    }

                }
                //处理奖励
                if (!string.IsNullOrEmpty(piece.reward))
                {
                    RewardManager.Instance.ApplyRewards(piece.reward);
                }

                //等待播放动画
                if (!string.IsNullOrEmpty(piece.playTimeline))
                {
                    TimelineManager.Instance.PlayTimeline();
                    yield return new WaitUntil(() => !TimelineManager.Instance.IsTimelinePlaying());
                }


                // 动态加载下一剧情文件
                if (!string.IsNullOrEmpty(piece.nextDialogueCSVFileName))
                {
                    Debug.Log("加载下一剧情：" + piece.nextDialogueCSVFileName);

                    BlackScreenManager.Instance.TransionBlackScreenSortOrder(100);
                    yield return BlackScreenManager.Instance.FadeIn(Settings.fadeDuration, false);
                    SetAllFalse();

                    yield return BlackScreenManager.Instance.FadeOut(Settings.fadeDuration, false);
                    BlackScreenManager.Instance.TransionBlackScreenSortOrder(0);

                    // 动态加载CSV
                    //TextAsset nextCSV = DialogueCSVReader.LoadCSVFromResources(piece.nextDialogueCSVFileName);
                    // if (nextCSV != null)
                    // {
                    StoryProgressManager.Instance.MarkStoryAsCompleted(piece.belongToCSVFileName);
                    //var newDialogueList = DialogueCSVReader.Instance.LoadDialogueData(nextCSV);
                    EventHandler.CallStartNewDialogueEvent(piece.nextDialogueCSVFileName);
                    //}
                    yield break;
                }
                // 动态加载下一剧情文件
                if (!string.IsNullOrEmpty(piece.nextIndex))
                {
                    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
                    Debug.Log("跳转下一剧情：" + piece.nextIndex);

                    SetAllFalse();
                    EventHandler.CallLoadDialogueByIndex(piece.nextIndex, piece.belongToCSVFileName);
                    yield break;
                }
            }
            piece.isDone = true;

            if (piece.isfinalNotFirst == 1)
            {
                StoryProgressManager.Instance.MarkStoryAsCompleted(piece.belongToCSVFileName);
                PlayerController.Instance.movement.SetPause(false);
                yield break;
            }

            continueButton.gameObject.SetActive(piece.hasToPause && piece.isDone);
        }
        else
        {
            SetAllFalse();
            EventHandler.HaveOnFocusCamear();
            PlayerController.Instance.movement.SetPause(false);
        }
    }

    public void SetAllFalse()
    {
        //IsDialogueActive = false;
        // 隐藏所有UI（无对话时）
        dialogueBoxTop.SetActive(false);
        dialogueBoxBottom.SetActive(false);
        nameLeft.gameObject.SetActive(false);
        nameRight.gameObject.SetActive(false);
        faceLeft.gameObject.SetActive(false);
        faceRight.gameObject.SetActive(false);
        dialogueText.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);

        faceLeft.sprite = null;
        emotionLeftImage.sprite = null;
        nameLeft.text = "";

        currentPiece = null;
    }

    public IEnumerator AnimateText(string text, float dely)//dely为单个字符打出时间
    {
        if (text.Contains("x"))
            text = text.Replace("x", "□");
        dialogueText.text = "";
        for (int i = 0; i <= text.Length; i++)
        {
            // 如果玩家按了空格或点击，立即显示完整文本
            if (Input.GetMouseButtonDown(0))
            {
                dialogueText.text = text;
                break;
            }

            dialogueText.text = text.Substring(0, i);
            yield return new WaitForSeconds(dely);
        }
    }
    public void ContinueDialogue()
    {
        if (isAnimating)
        {
            // 如果正在动画，立即完成动画
            StopAllCoroutines();
            dialogueText.text = currentPiece.dialogueText;
            isAnimating = false;
            currentPiece.isDone = true;
            continueButton.gameObject.SetActive(currentPiece.hasToPause);
        }
        else if (currentPiece != null && currentPiece.isDone)
        {
            continueButton.gameObject.SetActive(false);
            EventHandler.TriggerNextDialogue();
        }
    }


    private IEnumerator SettleOptions()
    {
        DialoguePiece piece = currentPiece;

        List<Button> optionButtons = new List<Button>();

        // 检查是否有前置条件
        bool hasPreconditions = !string.IsNullOrEmpty(piece.prerequisites);
        string[] preconditions = hasPreconditions ? piece.prerequisites.Split('|') : new string[0];

        for (int i = 0; i < piece.option.Count; i++)
        {
            bool shouldShow = true;

            // 只有存在前置条件时才检查
            if (hasPreconditions && i < preconditions.Length && !string.IsNullOrEmpty(preconditions[i]))
            {
                shouldShow = ConditionSystem.CheckAll(preconditions[i]);
            }

            if (shouldShow)
            {
                Button optionButton = Instantiate(optionButtonPrefab, optionsPanel.transform);
                // 设置图像为原始尺寸
                optionButton.image.SetNativeSize();
                optionButton.image.type = Image.Type.Sliced;
                // 获取 RectTransform
                RectTransform rectTransform = optionButton.image.GetComponent<RectTransform>();
                // 计算新高度（增加10%）
                float newHeight = rectTransform.rect.height * 1.1f;
                // 使用 SetSizeWithCurrentAnchors 调整高度，保持宽度不变
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rectTransform.rect.width);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);

                optionButton.GetComponentInChildren<TextMeshProUGUI>().text = piece.option[i];
                int currentOptionIndex = i;
                optionButton.onClick.AddListener(() => OnOptionSelected(currentOptionIndex));
                optionButtons.Add(optionButton);
            }
        }

        // 如果没有可显示的选项，自动继续
        if (optionButtons.Count == 0)
        {
            selectedOptionIndex = 0;
        }
        else
        {
            optionMove.SetBool("existoption", true);
            optionMove.SetBool("selected", false);

            while (selectedOptionIndex == -1)
            {
                yield return null;
            }

            optionMove.SetBool("selected", true);
            optionMove.SetBool("existoption", false);
            yield return new WaitForSeconds(0.5f);
        }

        foreach (Button button in optionButtons)
        {
            Destroy(button.gameObject);
        }

        ProcessOption(selectedOptionIndex, piece.option);
        selectedOptionIndex = -1;
    }
    // 选项选择回调
    private void OnOptionSelected(int index)
    {
        Debug.Log("选择：" + index);
        selectedOptionIndex = index;
    }

    private bool ProcessOption(int optionIndex, List<string> options)
    {
        if (string.IsNullOrEmpty(currentPiece.nextIndex))
        {
            dialogueText.text = options[optionIndex];
            return true;
        }
        else
        {
            string[] nextIndices = currentPiece.nextIndex.Split('|');
            if (optionIndex < nextIndices.Length)
            {
                EventHandler.CallLoadDialogueByIndex(nextIndices[optionIndex], currentPiece.belongToCSVFileName);
                selectedOptionIndex = -1;
            }
            return false;
        }
    }
    private void SetImageColor(bool isLeft, Color color)
    {
        if (isLeft)//左边亮
        {
            faceLeft.color = new Color(1f, 1f, 1f);
            emotionLeftImage.color = new Color(1f, 1f, 1f);
            faceRight.color = color;//将图片变灰
            emotionRightImage.color = color;//将图片变灰
        }
        else//右边亮
        {
            faceRight.color = new Color(1f, 1f, 1f);
            emotionRightImage.color = new Color(1f, 1f, 1f);
            faceLeft.color = color;
            emotionLeftImage.color = color;
        }

    }
}