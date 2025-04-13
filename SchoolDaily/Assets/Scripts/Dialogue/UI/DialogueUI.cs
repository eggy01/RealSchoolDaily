using System.Collections;
using System.Collections.Generic;
using SchoolD.Dialogue;
using TMPro;
using UnityEngine;
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
                dialogueBoxTop.SetActive(true);
                dialogueText.gameObject.SetActive(true);
                dialogueText.text = piece.dialogueText; // 直接显示完整文本（不逐字动画）

                // 等待点击继续
                while (!Input.GetMouseButtonDown(0)) // 假设点击鼠标左键继续
                {
                    yield return null;
                }

                piece.isDone = true;
                yield break; // 结束协程
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

                    if (piece.name.Equals("主角"))
                    {
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

                    // 设置对话框样式（左右对话）
                    if (piece.onLeft)
                    {
                        dialogueBoxTop.GetComponent<Image>().sprite = leftDialogSprite;
                        dialogueBoxBottom.GetComponent<Image>().sprite = rightDialogSprite;
                        SetImageColor(true, new Color(0.3f, 0.3f, 0.3f)); // 左边亮
                    }
                    else
                    {
                        dialogueBoxTop.GetComponent<Image>().sprite = rightDialogSprite;
                        dialogueBoxBottom.GetComponent<Image>().sprite = leftDialogSprite;
                        SetImageColor(false, Settings.DialogueInactiveColor); // 右边亮
                    }
                }

                // 处理选项或逐字动画
                if (piece.option != null && piece.option.Count > 0)
                {
                    dialogueText.gameObject.SetActive(false);
                    List<Button> optionButtons = new List<Button>();
                    for (int i = 0; i < piece.option.Count; i++)
                    {
                        Button optionButton = Instantiate(optionButtonPrefab, optionsPanel.transform);
                        optionButton.image.SetNativeSize();
                        optionButton.GetComponentInChildren<TextMeshProUGUI>().text = piece.option[i];
                        int currentOptionIndex = i;
                        optionButton.onClick.AddListener(() => OnOptionSelected(currentOptionIndex));
                        optionButtons.Add(optionButton);
                    }

                    while (selectedOptionIndex == -1)
                    {
                        yield return null;
                    }

                    foreach (Button button in optionButtons)
                    {
                        Destroy(button.gameObject);
                    }

                    ProcessOption(selectedOptionIndex, piece.option);
                    selectedOptionIndex = -1;
                }
                else
                {
                    yield return StartCoroutine(AnimateText(piece.dialogueText, 1f)); // 逐字动画
                }
            }
            if (piece.nextDialogue != null && !piece.nextDialogue.Equals(string.Empty))//有紧接着的下一段剧情
            {

            }

            piece.isDone = true;
            continueButton.gameObject.SetActive(piece.hasToPause && piece.isDone);
        }
        else
        {
            // 隐藏所有UI（无对话时）
            dialogueBoxTop.SetActive(false);
            dialogueBoxBottom.SetActive(false);
            nameLeft.gameObject.SetActive(false);
            nameRight.gameObject.SetActive(false);
            faceLeft.gameObject.SetActive(false);
            faceRight.gameObject.SetActive(false);
            dialogueText.gameObject.SetActive(false);
            continueButton.gameObject.SetActive(false);
            currentPiece = null;
        }
    }

    IEnumerator AnimateText(string text, float duration)
    {
        dialogueText.text = "";
        float lettersPerSecond = text.Length / duration;

        for (int i = 0; i <= text.Length; i++)
        {
            // 如果玩家按了空格或点击，立即显示完整文本
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
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

    // 选项选择回调
    private void OnOptionSelected(int index)
    {
        selectedOptionIndex = index;
    }

    // 处理选项结果
    private void ProcessOption(int optionIndex, List<string> options)
    {
        Debug.Log("玩家选择了选项：" + optionIndex);
        dialogueText.text = options[optionIndex];
        dialogueText.gameObject.SetActive(true);


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
            faceRight.color = color;
            emotionRightImage.color = color;
        }

    }
}