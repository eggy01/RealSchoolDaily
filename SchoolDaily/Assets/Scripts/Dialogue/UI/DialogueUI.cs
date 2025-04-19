using System.Collections;
using System.Collections.Generic;
using SchoolD.Dialogue;
using TMPro;
using UnityEngine;
using UnityEngine.iOS;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
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
<<<<<<< Updated upstream
=======

    public GameObject taskPanel; // 存放任务提示的面板
    public Animator LeftoptionMove;
    private bool isblack = false;
    private string moveToPosition;

<<<<<<< Updated upstream
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
=======
    public int nexePieceIndex = -1;
>>>>>>> Stashed changes
    private string MoveToPosition;

>>>>>>> Stashed changes
    private void Awake()
    {
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

        if (piece != null)
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
            if (piece.name.Equals("旁白"))
            {
                dialogueBoxBottom.SetActive(true);
                dialogueText.gameObject.SetActive(true);

                // yield return StartCoroutine(AnimateText(piece.dialogueText, 1f)); // 逐字动画
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
                        nameLeft.text = piece.name;
                        faceLeft.sprite = piece.faceImage;

                        if (!faceLeft.sprite.name.Equals("默认2"))
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
<<<<<<< Updated upstream

                // 处理选项或逐字动画
                if (piece.option != null && piece.option.Count > 0)
                {
                    dialogueText.gameObject.SetActive(false);
                    List<Button> optionButtons = new List<Button>();
                    for (int i = 0; i < piece.option.Count; i++)
=======
            }
            if (!string.IsNullOrEmpty(piece.dialogueText))
                yield return StartCoroutine(AnimateText(piece.dialogueText, 1f));

            //场景切换，人物移动
            if (!string.IsNullOrEmpty(piece.MoveToPosition))
            {
                MoveToPosition = piece.MoveToPosition;
            }
            // 处理选项或逐字动画
            if (piece.option != null && piece.option.Count > 0)
            {
                List<Button> optionButtons = new List<Button>();

                // 检查是否有前置条件
                bool hasPreconditions = !string.IsNullOrEmpty(piece.prerequisites);
                string[] preconditions = hasPreconditions ? piece.prerequisites.Split('|') : new string[0];

                Debug.Log("前置条件" + preconditions.Length);
                for (int i = 0; i < piece.option.Count; i++)
                {
                    bool shouldShow = true;

                    // 只有存在前置条件时才检查
                    if (hasPreconditions && i < preconditions.Length && !string.IsNullOrEmpty(preconditions[i]))
                    {
                        Debug.Log(preconditions[i]);
                        shouldShow = ConditionSystem.Check(preconditions[i]);
                        Debug.Log("满足：" + shouldShow);
                    }

                    if (shouldShow)
>>>>>>> Stashed changes
                    {
                        Button optionButton = Instantiate(optionButtonPrefab, optionsPanel.transform);
                        optionButton.image.SetNativeSize();
                        optionButton.GetComponentInChildren<TextMeshProUGUI>().text = piece.option[i];
                        int currentOptionIndex = i;
                        optionButton.onClick.AddListener(() => OnOptionSelected(currentOptionIndex));
                        optionButtons.Add(optionButton);
                    }
<<<<<<< Updated upstream
=======
                }

                // 如果没有可显示的选项，自动继续
                if (optionButtons.Count == 0)
                {
                    selectedOptionIndex = 0;
                }
                else
                {
>>>>>>> Stashed changes
                    optionMove.SetBool("existoption", true);
                    optionMove.SetBool("selected", false);

                    while (selectedOptionIndex == -1)
                    {
                        yield return null;
                    }
                    if (selectedOptionIndex != -1)
                    {
                        optionMove.SetBool("selected", true);
                        optionMove.SetBool("existoption", false);
                        yield return new WaitForSeconds(0.5f);
                        foreach (Button button in optionButtons)
                        {

                            Destroy(button.gameObject);
                        }
                    }

                    ProcessOption(selectedOptionIndex, piece.option);
                    selectedOptionIndex = -1;
                }
                else
                {
                    yield return StartCoroutine(AnimateText(piece.dialogueText, 1f)); // 逐字动画
                }
<<<<<<< Updated upstream
=======

                if (!ProcessOption(selectedOptionIndex, piece.option))
                    yield break;
                selectedOptionIndex = -1;
            }

            //处理任务
            if (!string.IsNullOrEmpty(piece.taskPID))
            {
                LeftoptionMove.SetBool("haveNewTask", true);
                yield return new WaitForSeconds(0.5f);
                LeftoptionMove.SetBool("haveNewTask", false);

                // 查找TaskTipDetail子对象
                taskPanel.transform.Find("TaskTip/TaskTipDetail").gameObject.GetComponent<TextMeshProUGUI>().text = TaskSystem.Instance.GetTask(piece.taskPID).description;
                //EventHandler.callHasNewTaskEvent(TaskSystem.Instance.GetTask(piece.taskPID));
>>>>>>> Stashed changes
            }

            // 动态加载下一剧情文件
            if (!string.IsNullOrEmpty(piece.nextDialogueCSVFileName))
            {
                BlackScreenManager.Instance.TransionBlackScreenSortOrder(100);
                yield return BlackScreenManager.Instance.FadeIn(Settings.fadeDuration, false);

                SetAllFalse();

                yield return BlackScreenManager.Instance.FadeOut(Settings.fadeDuration, false);
                BlackScreenManager.Instance.TransionBlackScreenSortOrder(0);


                // 动态加载CSV
                TextAsset nextCSV = DialogueCSVReader.LoadCSVFromResources(piece.nextDialogueCSVFileName);
                if (nextCSV != null)
                {
                    var newDialogueList = DialogueCSVReader.Instance.LoadDialogueData(nextCSV);
                    EventHandler.CallStartNewDialogueEvent(newDialogueList, piece.nextDialogueCSVFileName);
                }
                yield break;
            }

<<<<<<< Updated upstream
<<<<<<< Updated upstream
            if (!string.IsNullOrEmpty(piece.moveToPosition))
            {
                moveToPosition = piece.moveToPosition;
            }
=======
            //场景切换，人物移动
            if (!string.IsNullOrEmpty(piece.MoveToPosition))
                MoveToPosition = piece.MoveToPosition;
>>>>>>> Stashed changes
=======

>>>>>>> Stashed changes

            piece.isDone = true;
            continueButton.gameObject.SetActive(piece.hasToPause && piece.isDone);
        }
        else
        {
<<<<<<< Updated upstream
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
=======
            SetAllFalse();
<<<<<<< Updated upstream
            if (!string.IsNullOrEmpty(moveToPosition))
                EventHandler.CallTransitionEvent(moveToPosition, SceneToInitalPosition.Instance.GetInitialPosition(moveToPosition));
            moveToPosition = "";
>>>>>>> Stashed changes
=======

            if (!string.IsNullOrEmpty(MoveToPosition)) //场景切换，人物移动
                EventHandler.CallTransitionEvent(MoveToPosition, SceneToInitialPosition.Instance.GetInitialPosition(MoveToPosition));
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
            }

>>>>>>> Stashed changes
        }
    }

    IEnumerator AnimateText(string text, float duration)
    {
        dialogueText.text = "";
        float lettersPerSecond = text.Length / duration;

        for (int i = 0; i <= text.Length; i++)
        {
            // 如果玩家按了空格或点击，立即显示完整文本
            if (Input.GetMouseButtonDown(0))
            {
                dialogueText.text = text;
                break;
            }

            dialogueText.text = text.Substring(0, i);
            yield return new WaitForSeconds(1f / lettersPerSecond);
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
                optionButton.image.SetNativeSize();
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
        selectedOptionIndex = index;
    }

<<<<<<< Updated upstream
    // 处理选项结果
    private void ProcessOption(int optionIndex, List<string> options)
    {
        // Debug.Log("玩家选择了选项：" + optionIndex);
        dialogueText.text = options[optionIndex];

        dialogueText.gameObject.SetActive(true);
=======
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
                EventHandler.CallLoadDialogueByIndex(nextIndices[optionIndex]);
                selectedOptionIndex = -1;
            }
            return false;
        }
>>>>>>> Stashed changes
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