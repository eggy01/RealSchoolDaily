using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System.Text.RegularExpressions;
using System;
using SchoolD.Dialogue;

public class ChatSystem : MonoBehaviour
{
    public static ChatSystem Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject phoneChatPopup;

    [Header("Navigation")]
    [SerializeField] private Button newMessageButton;
    [SerializeField] private Button friendsButton;
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject newMessageBadge;

    [Header("List Views")]
    [SerializeField] private Transform newMessagesContainer;
    [SerializeField] private Transform friendsContainer;
    [SerializeField] private GameObject messageItemPrefab;
    [SerializeField] private GameObject friendItemPrefab;

    [Header("Chat View")]
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private TextMeshProUGUI chatGroupNameText;
    [SerializeField] private Image chatAvatar;
    [SerializeField] private TextMeshProUGUI chatNameText;
    [SerializeField] private Transform chatContent;
    [SerializeField] private ScrollRect chatScrollRect;
    [SerializeField] private GameObject messagePrefab;
    [SerializeField] private GameObject playerMessagePrefab;
    [SerializeField] private GameObject timeLabelPrefab;

    [Header("Options")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Button optionButtonPrefab;
    [SerializeField] private float optionDisplayDelay = 0.5f;

    // Data structures
    private Dictionary<string, List<ChatRecord>> conversations = new Dictionary<string, List<ChatRecord>>();
    private Dictionary<string, bool> unreadMessages = new Dictionary<string, bool>();
    private string currentChattingGroup;
    private int selectedOptionIndex = -1;
    private bool isInPhoneMode = false;

    [System.Serializable]
    public class ChatRecord
    {
        public string senderName;
        public string message;
        public DateTime timestamp;
        public bool isPlayerMessage;

        public bool ShouldShowTimeLabel(ChatRecord previousRecord)
        {
            if (previousRecord == null) return true;
            return (timestamp - previousRecord.timestamp).TotalMinutes >= 30;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitializeUI();
    }

    private void InitializeUI()
    {
        mainPanel.SetActive(false);
        //phoneChatPopup.SetActive(false);
        chatPanel.SetActive(false);
        optionsPanel.SetActive(false);

        newMessageButton.onClick.AddListener(ShowNewMessagesView);
        friendsButton.onClick.AddListener(ShowFriendsView);
        backButton.onClick.AddListener(ReturnToMainView);
    }

    private void Update()//
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            ToggleMainInterface();
        }
    }

    #region Public Interface Methods
    public void ToggleMainInterface()
    {
        if (mainPanel.activeSelf)
        {
            HideAll();
        }
        else
        {
            ShowMainInterface();
        }
    }

    public void ShowMainInterface()
    {
        isInPhoneMode = false;
        mainPanel.SetActive(true);
        //phoneChatPopup.SetActive(false);
        ShowNewMessagesView();
    }

    public void HideAll()
    {
        mainPanel.SetActive(false);
        phoneChatPopup.SetActive(false);
    }

    public IEnumerator ShowPhoneMessage(DialoguePiece piece)
    {
        // Pause game
        PlayerController.Instance?.movement?.SetPause(true);

        // Setup phone mode
        isInPhoneMode = true;
        mainPanel.SetActive(true);
        //phoneChatPopup.SetActive(true);

        // Parse message info
        ParseMessageInfo(piece, out string groupName, out string senderName);
        Debug.Log("群名");
        Debug.Log("说话人名字");
        // Setup chat header
        currentChattingGroup = groupName;//组群名字
        chatNameText.text = senderName;//说话人名字
        chatGroupNameText.text = groupName;

        // Load chat history
        ClearChatContent();
        LoadChatHistory(groupName);
        chatPanel.SetActive(true);

        // Add new message
        AddNewMessageToChat(piece);
        SaveMessageToHistory(piece, groupName, senderName);
        MarkAsUnread(groupName);

        // Handle options if any
        if (piece.option != null && piece.option.Count > 0)
        {
            yield return StartCoroutine(ShowOptions(piece));
        }
        else
        {
            yield return WaitForPlayerContinue();
        }

        // Clean up
        if (isInPhoneMode)
        {
            HideAll();
            PlayerController.Instance?.movement?.SetPause(false);
        }
    }
    #endregion

    #region Navigation Methods
    private void ShowNewMessagesView()
    {
        chatPanel.SetActive(false);//关闭聊天框
        ClearContainer(newMessagesContainer);//清理

        foreach (var entry in unreadMessages.Where(e => e.Value))
        {
            if (conversations.TryGetValue(entry.Key, out var messages) && messages.Count > 0)
            {
                var lastMessage = messages.Last();
                CreateMessageListItem(entry.Key, lastMessage.message, newMessagesContainer);
            }
        }

        UpdateNewMessageBadge();
    }

    private void ShowFriendsView()
    {
        chatPanel.SetActive(false);
        ClearContainer(friendsContainer);

        foreach (var group in conversations.Keys)
        {
            if (conversations[group].Count > 0)
            {
                var lastMessage = conversations[group].Last();
                CreateFriendListItem(group, lastMessage.message, friendsContainer);
            }
        }
    }

    public void OpenChatWithGroup(string groupName)
    {
        currentChattingGroup = groupName;
        MarkAsRead(groupName);

        chatPanel.SetActive(true);
        chatNameText.text = groupName;
        chatGroupNameText.text = groupName;

        ClearChatContent();
        LoadChatHistory(groupName);
        ScrollToBottom();
        UpdateNewMessageBadge();//设置红点
    }

    private void ReturnToMainView()
    {
        if (isInPhoneMode)
        {
            HideAll();
            PlayerController.Instance?.movement?.SetPause(false);
        }
        else
        {
            ShowNewMessagesView();
        }
    }
    #endregion

    #region Message Handling
    private void ParseMessageInfo(DialoguePiece piece, out string groupName, out string senderName)
    {
        // 默认值
        groupName = "Default Group";
        senderName = "";

        // 主要匹配模式：手机屏幕（群组名）发送者
        var match = Regex.Match(piece.name, @"手机屏幕[\(（](.+?)[\)）](.+)");
        if (!match.Success)
        {
            // 备用匹配模式：群组名发送者
            match = Regex.Match(piece.name, @"(.+群)(.+)");
        }

        if (match.Success && match.Groups.Count >= 3)
        {
            groupName = match.Groups[1].Value.Trim();
            senderName = match.Groups[2].Value.Trim();

            // 调试输出
            Debug.Log($"原始字符串: {piece.name}");
            Debug.Log($"匹配结果: 群组='{groupName}', 发送者='{senderName}'");
        }
        else
        {
            Debug.LogWarning($"无法解析消息格式: {piece.name}");
        }
    }

    private void AddNewMessageToChat(DialoguePiece piece)
    {
        ParseMessageInfo(piece, out string groupName, out string senderName);

        bool isPlayer = senderName == Settings.playerName;

        var newRecord = new ChatRecord
        {
            senderName = senderName,
            message = piece.dialogueText,
            timestamp = DateTime.Now,
            isPlayerMessage = isPlayer
        };

        AddMessageToChat(newRecord, GetPreviousMessage(groupName));
        ScrollToBottom();
    }

    private void AddMessageToChat(ChatRecord record, ChatRecord previousRecord = null)
    {
        if (record.ShouldShowTimeLabel(previousRecord))
        {
            CreateTimeLabel(record.timestamp);
        }

        GameObject prefab = record.isPlayerMessage ? playerMessagePrefab : messagePrefab;
        var messageObj = Instantiate(prefab, chatContent);
        var messageItem = messageObj.GetComponent<ChatMessageItem>();

        if (messageItem != null)
        {
            messageItem.Initialize(record.senderName, record.message, !record.isPlayerMessage);

            if (DialogueCSVReader.Instance?.spriteDict?.TryGetValue(record.senderName, out Sprite avatar) ?? false)
            {
                messageItem.SetAvatar(avatar);
            }
        }
    }

    private void CreateTimeLabel(DateTime time)
    {
        var timeLabel = Instantiate(timeLabelPrefab, chatContent);
        var timeLabelItem = timeLabel.GetComponent<ChatMessageItem>();
        timeLabelItem.InitializeAsTimeLabel(time.ToString("HH:mm"));
    }

    private void SaveMessageToHistory(DialoguePiece piece, string groupName, string senderName)
    {
        bool isPlayer = senderName == Settings.playerName;

        var newRecord = new ChatRecord
        {
            senderName = senderName,
            message = piece.dialogueText,
            timestamp = DateTime.Now,
            isPlayerMessage = isPlayer
        };

        if (!conversations.ContainsKey(groupName))
        {
            conversations[groupName] = new List<ChatRecord>();
        }
        conversations[groupName].Add(newRecord);
    }

    private ChatRecord GetPreviousMessage(string groupName)
    {
        if (conversations.TryGetValue(groupName, out var messages) && messages.Count >= 2)
        {
            return messages[messages.Count - 2];
        }
        return null;
    }
    #endregion

    #region Option Handling
    private IEnumerator ShowOptions(DialoguePiece piece)
    {
        yield return new WaitForSeconds(optionDisplayDelay);

        ClearOptions();
        var optionButtons = CreateOptionButtons(piece);

        if (optionButtons.Count == 0)
        {
            selectedOptionIndex = 0;
        }
        else
        {
            optionsPanel.SetActive(true);
            selectedOptionIndex = -1;

            while (selectedOptionIndex == -1)
            {
                yield return null;
            }

            optionsPanel.SetActive(false);
        }

        CleanupOptions(optionButtons);
        ProcessOptionResult(piece);
    }

    private List<Button> CreateOptionButtons(DialoguePiece piece)
    {
        List<Button> optionButtons = new List<Button>();

        for (int i = 0; i < piece.option.Count; i++)
        {
            if (!ShouldShowOption(piece, i)) continue;

            Button optionButton = Instantiate(optionButtonPrefab, optionsPanel.transform);
            optionButton.GetComponentInChildren<TextMeshProUGUI>().text = piece.option[i];

            int index = i;
            optionButton.onClick.AddListener(() => OnOptionSelected(index));

            optionButtons.Add(optionButton);
        }

        return optionButtons;
    }

    private bool ShouldShowOption(DialoguePiece piece, int optionIndex)
    {
        if (string.IsNullOrEmpty(piece.prerequisites)) return true;

        string[] preconditions = piece.prerequisites.Split('|');
        return optionIndex < preconditions.Length && ConditionSystem.Check(preconditions[optionIndex]);
    }

    private void ProcessOptionResult(DialoguePiece piece)
    {
        // Add player's choice to chat
        var optionPiece = new DialoguePiece
        {
            name = Settings.playerName,
            dialogueText = piece.option[selectedOptionIndex],
            onLeft = false
        };

        AddNewMessageToChat(optionPiece);
        SaveMessageToHistory(optionPiece, currentChattingGroup, Settings.playerName);

        // Handle dialogue continuation
        if (!string.IsNullOrEmpty(piece.nextIndex))
        {
            string[] nextIndices = piece.nextIndex.Split('|');
            if (selectedOptionIndex < nextIndices.Length)
            {
                EventHandler.CallLoadDialogueByIndex(nextIndices[selectedOptionIndex], piece.belongToCSVFileName);
            }
        }
    }

    private void OnOptionSelected(int index)
    {
        selectedOptionIndex = index;
    }
    #endregion

    #region Utility Methods
    private void LoadChatHistory(string groupName)
    {
        if (conversations.TryGetValue(groupName, out var messages))
        {
            ChatRecord previousRecord = null;
            foreach (var record in messages)
            {
                AddMessageToChat(record, previousRecord);
                previousRecord = record;
            }
        }
    }

    private void ClearChatContent()
    {
        foreach (Transform child in chatContent)
        {
            Destroy(child.gameObject);
        }
    }

    private void ClearContainer(Transform container)
    {
        foreach (Transform child in container)
        {
            if (child != container) // Don't destroy the container itself
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void ClearOptions()
    {
        foreach (Transform child in optionsPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void CleanupOptions(List<Button> optionButtons)
    {
        foreach (Button button in optionButtons)
        {
            if (button != null)
            {
                Destroy(button.gameObject);
            }
        }
    }

    private IEnumerator WaitForPlayerContinue()
    {
        bool waiting = true;
        while (waiting)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                waiting = false;
            }
            yield return null;
        }
    }

    private void CreateMessageListItem(string groupName, string lastMessage, Transform parent)
    {
        var itemObj = Instantiate(messageItemPrefab, parent);
        var item = itemObj.GetComponent<MessageListItem>();

        if (item != null)
        {
            bool isUnread = unreadMessages.TryGetValue(groupName, out bool unread) && unread;
            item.Initialize(
                groupName,
                lastMessage,
                isUnread,
                () => OpenChatWithGroup(groupName)
            );

            if (DialogueCSVReader.Instance?.spriteDict?.TryGetValue(groupName, out Sprite avatar) ?? false)
            {
                item.SetAvatar(avatar);
            }
        }
    }

    private void CreateFriendListItem(string groupName, string lastMessage, Transform parent)
    {
        var itemObj = Instantiate(friendItemPrefab, parent);
        var item = itemObj.GetComponent<FriendListItem>();

        if (item != null)
        {
            bool isUnread = unreadMessages.TryGetValue(groupName, out bool unread) && unread;
            item.Initialize(
                groupName,
                isUnread,
                () => OpenChatWithGroup(groupName)
            );

            if (DialogueCSVReader.Instance?.spriteDict?.TryGetValue(groupName, out Sprite avatar) ?? false)
            {
                item.SetAvatar(avatar);
            }
        }
    }

    private void MarkAsUnread(string groupName)
    {
        unreadMessages[groupName] = true;
        UpdateNewMessageBadge();
    }

    private void MarkAsRead(string groupName)
    {
        unreadMessages[groupName] = false;
        UpdateNewMessageBadge();
    }

    private void UpdateNewMessageBadge()
    {
        newMessageBadge.SetActive(unreadMessages.Values.Any(unread => unread));
    }

    private void ScrollToBottom()
    {
        StartCoroutine(ScrollToBottomCoroutine());
    }

    private IEnumerator ScrollToBottomCoroutine()
    {
        yield return new WaitForEndOfFrame();
        if (chatScrollRect != null)
        {
            chatScrollRect.verticalNormalizedPosition = 0f;
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)chatContent);
        }
    }
    #endregion
}