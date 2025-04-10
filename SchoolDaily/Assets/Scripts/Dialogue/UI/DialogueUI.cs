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
    public TextMeshProUGUI nameRight, nameLeft;//对话人物名称
    public Button continueButton;//继续按钮的Button组件

    public Sprite leftDialogSprite;
    public Sprite rightDialogSprite;
    private DialoguePiece currentPiece;
    private bool isAnimating = false;
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

            dialogueBoxTop.SetActive(true);
            dialogueBoxBottom.SetActive(true);
            continueButton.gameObject.SetActive(false);
            nameLeft.gameObject.SetActive(true);
            nameRight.gameObject.SetActive(true);
            faceLeft.gameObject.SetActive(true);
            faceRight.gameObject.SetActive(true);
            dialogueText.gameObject.SetActive(true);

            dialogueText.text = string.Empty;

            if (piece.name != string.Empty)
            {
                if (piece.name.Equals("主角"))
                {
                    nameRight.text = piece.name;
                }
                else
                {
                    nameLeft.text = piece.name;
                    faceLeft.sprite = piece.faceImage;
                }
                if (piece.onLeft)//左边人说话
                {
                    dialogueBoxTop.GetComponent<Image>().sprite = leftDialogSprite;
                    dialogueBoxBottom.GetComponent<Image>().sprite = rightDialogSprite;

                    faceLeft.transform.SetSiblingIndex(5);//将左边的图片置于最上面
                    faceRight.transform.SetSiblingIndex(0);//将右边的图片置于最下面

                    faceLeft.color = new Color(1, 1, 1);
                    faceRight.color = new Color(0.3f, 0.3f, 0.3f);//将图片变灰

                }
                else//右边人说话
                {
                    dialogueBoxTop.GetComponent<Image>().sprite = rightDialogSprite;
                    dialogueBoxBottom.GetComponent<Image>().sprite = leftDialogSprite;
                    faceLeft.transform.SetSiblingIndex(0);//将左边的图片置于最下面
                    faceRight.transform.SetSiblingIndex(5);//将右边的图片置于最上面

                    faceLeft.color = new Color(0.3f, 0.3f, 0.3f);//将图片变灰
                    faceRight.color = new Color(1, 1, 1);

                }
            }
            else
            {
                dialogueBoxTop.SetActive(false);
                dialogueBoxBottom.SetActive(false);
                nameLeft.gameObject.SetActive(false);
                nameRight.gameObject.SetActive(false);
                faceLeft.gameObject.SetActive(false);
                faceRight.gameObject.SetActive(false);
                dialogueText.gameObject.SetActive(false);

                continueButton.gameObject.SetActive(false);
            }

            isAnimating = true;
            yield return StartCoroutine(AnimateText(piece.dialogueText, 1f));
            isAnimating = false;

            piece.isDone = true;

            if (piece.hasToPause && piece.isDone)
            {
                continueButton.gameObject.SetActive(true);
            }
            else
            {
                continueButton.gameObject.SetActive(false);
            }
        }
        else
        {
            dialogueBoxTop.SetActive(false);
            dialogueBoxBottom.SetActive(false);
            nameLeft.gameObject.SetActive(false);
            nameRight.gameObject.SetActive(false);
            faceLeft.gameObject.SetActive(false);
            faceRight.gameObject.SetActive(false);
            dialogueText.gameObject.SetActive(false);

            continueButton.gameObject.SetActive(false);
            currentPiece = null;
            yield break;
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
}