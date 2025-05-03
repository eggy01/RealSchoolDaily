using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NQChartUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    public TextMeshProUGUI dialogueText;//对话框文本
    public Image faceRight, faceLeft;//对话框人物图片
    public TextMeshProUGUI nameRight, nameLeft;//对话人物名称

    public Sprite leftDialogSprite;
    public Sprite rightDialogSprite;

    public GameObject optionsPanel; // 存放选项按钮的面板
    public Button optionButtonPrefab; // 选项按钮预制体

    private int selectedOptionIndex = -1; // 记录玩家选择的选项索引

    public Animator optionMove;

    public int nexePieceIndex = -1;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
