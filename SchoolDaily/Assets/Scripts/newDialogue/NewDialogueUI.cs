using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SchoolD.NewDialogue.DialogueData;

namespace SchoolD.NewDialogue
{
    public class NewDialogueUI : MonoBehaviour
    {
        [Header("UI Components")]
        public Image leftCharacter;    // NPC角色（固定左侧）
        public Image rightCharacter;   // 玩家角色（固定右侧）
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI dialogueText;
        public Transform choicesPanel;
        public GameObject dialoguePanel;
        public Button continueButton;

        [Header("Settings")]
        public float textSpeed = 20f;
        public Color inactiveColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        public Sprite defaultPlayerPortrait; // 默认玩家立绘



        // 运行时状态
        private bool isTyping = false;
        private Coroutine typingCoroutine;
        private DialogueChoice selectedChoice;
        private DialogueNode currentNode;

        void Awake()
        {
            continueButton.onClick.AddListener(OnContinueClicked);
            dialoguePanel.SetActive(false);
        }

        public IEnumerator ShowDialogueNode(DialogueNode node)
        {
            currentNode = node;
            dialoguePanel.SetActive(true);

            // 设置角色显示（强制玩家在右，NPC在左）
            SetupCharacterDisplay(node.character);

            // 显示对话文本
            yield return StartCoroutine(TypeText(node.dialogueText));

            // // 处理选项或继续
            // if (node.choices.Count > 0)
            // {
            //     yield return StartCoroutine(ShowChoices(node.choices));
            // }
            // else
            // {
            //     yield return WaitForContinueInput();
            // }
        }

        private void SetupCharacterDisplay(DialogueData.CharacterInfo character)
        {
            // 重置状态
            leftCharacter.gameObject.SetActive(false);
            rightCharacter.gameObject.SetActive(false);
            nameText.text = "";

            if (character == null) return;

            // 判断是否是玩家角色
            bool isPlayer = character.isPlayer;

            if (isPlayer)
            {
                // 玩家角色（强制右侧）
                rightCharacter.gameObject.SetActive(true);
                rightCharacter.sprite = character.defaultPortrait ?? defaultPlayerPortrait;
                rightCharacter.color = Color.white;
                rightCharacter.rectTransform.anchoredPosition = character.portraitOffset;

                // 左侧NPC隐藏
                leftCharacter.gameObject.SetActive(false);
            }
            else
            {
                // NPC角色（强制左侧）
                leftCharacter.gameObject.SetActive(true);
                leftCharacter.sprite = character.defaultPortrait;
                leftCharacter.color = Color.white;
                leftCharacter.rectTransform.anchoredPosition = character.portraitOffset;

                // 右侧玩家变灰（如果可见）
                rightCharacter.color = inactiveColor;
            }

            // 设置角色名
            nameText.text = character.displayName;
            nameText.color = character.nameColor;
        }

        IEnumerator TypeText(string text)
        {
            dialogueText.text = "";
            isTyping = true;

            foreach (char c in text)
            {
                dialogueText.text += c;

                // 快速跳过
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
                {
                    dialogueText.text = text;
                    break;
                }

                yield return new WaitForSeconds(1f / textSpeed);
            }

            isTyping = false;
        }

        // IEnumerator ShowChoices(List<DialogueChoice> choices)
        // {
        //     var validChoices = choices.Where(c => CheckConditions(c.conditions)).ToList();

        //     if (validChoices.Count == 0)
        //     {
        //         Debug.Log("没有有效选项，自动继续");
        //         yield break;
        //     }

        //     foreach (var choice in validChoices)
        //     {
        //         var button = CreateChoiceButton(choice);
        //         yield return null; // 分帧实例化
        //     }

        //     selectedChoice = null;
        //     yield return new WaitUntil(() => selectedChoice != null);
        //     ClearChoices();

        //     if (!string.IsNullOrEmpty(selectedChoice.nextNode))
        //     {
        //         //DialogueManager.Instance.JumpToNode(selectedChoice.nextNode);
        //     }
        // }

        // Button CreateChoiceButton(DialogueChoice choice)
        // {
        //     Button button = Instantiate(choice.buttonPrefab ?? DialogueSystem.Instance.defaultChoiceButton,
        //                               choicesPanel);
        //     button.GetComponentInChildren<TextMeshProUGUI>().text = choice.choiceText;
        //     button.onClick.AddListener(() => selectedChoice = choice);
        //     return button;
        // }

        void ClearChoices()
        {
            foreach (Transform child in choicesPanel)
            {
                Destroy(child.gameObject);
            }
        }

        IEnumerator WaitForContinueInput()
        {
            continueButton.gameObject.SetActive(true);
            var wait = new WaitUntil(() => Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space));
            yield return wait;
            continueButton.gameObject.SetActive(false);
        }

        void OnContinueClicked()
        {
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                dialogueText.text = currentNode.dialogueText;
                isTyping = false;
            }
            else
            {
                DialogueSystem.Instance.ContinueDialogue();
            }
        }

        bool CheckConditions(List<Condition> conditions)
        {
            return conditions?.All(c => c.IsMet()) ?? true;
        }

        public void Hide()
        {
            dialoguePanel.SetActive(false);
            ClearChoices();
            continueButton.gameObject.SetActive(false);
        }
    }
}