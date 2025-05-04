using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System.Text.RegularExpressions;
using System;
using SchoolD.Dialogue;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.VisualScripting;
using SchoolD.Task;

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


    [System.Serializable]
    public class DeferredMessage
    {
        public DialoguePiece piece;
        public string groupName;
        public string senderName;
        public bool hasOptions;
        public string storyID;
        public bool isCompleted;
        public int pieceIndex; // 新增：片段在剧情中的索引
        public int totalPieces; // 新增：剧情总片段数
    }
    bool isProcessingDeferredMessages = false;

    private List<DeferredMessage> deferredMessages = new List<DeferredMessage>();
    //存档
    [System.Serializable]
    private class ChatSaveData
    {
        public Dictionary<string, List<ChatRecord>> conversations;
        public Dictionary<string, bool> unreadMessages;
        public List<DeferredMessage> deferredMessages; // 新增
    }

    private string chatSavePath;

    private void Awake()
    {
        chatSavePath = Path.Combine(Application.persistentDataPath, "chat_save.dat");
        Debug.Log("聊天存档路径: " + chatSavePath);
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        LoadChatData();//加载存档
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
    }

    private void Update()//
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            ToggleMainInterface();
        }
    }

    #region Public Interface Methods
    public void ToggleMainInterface()//按键弹出
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

    public void ShowMainInterface()//
    {
        isInPhoneMode = false;
        mainPanel.SetActive(true);
        //phoneChatPopup.SetActive(false);
        ShowNewMessagesView();
    }

    public void HideAll()
    {
        mainPanel.SetActive(false);
        //phoneChatPopup.SetActive(false);
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

        // Setup chat header
        if (groupName.Equals("NQChat"))
            groupName = senderName;
        currentChattingGroup = groupName;//组群名字
        //chatNameText.text = senderName;//说话人名字
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
        friendsContainer.gameObject.SetActive(false);
        newMessagesContainer.gameObject.SetActive(true);

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
        friendsContainer.gameObject.SetActive(true);
        newMessagesContainer.gameObject.SetActive(false);
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

    // 在ChatSystem类中添加以下方法

    /// <summary>
    /// 接收并存储延迟剧情消息
    /// </summary>
    public void ReceiveDeferredStory(string storyID)
    {
        int startIndex = 0;
        List<DialoguePiece> pieces = DialogueCSVReader.Instance.LoadDialogueData(DialogueLoader.Instance.LoadCSVFromResources(storyID));
        if (pieces == null || pieces.Count == 0)
        {
            Debug.LogWarning($"没有可用的对话片段，剧情ID: {storyID}");
            return;
        }

        // 解析第一个片段的信息获取群组和发送者
        ParseMessageInfo(pieces[0], out string groupName, out string senderName);
        // 存储所有片段
        for (int i = startIndex; i < pieces.Count; i++)
        {
            var currentPiece = pieces[i]; // 使用currentPiece避免命名冲突

            var deferredMsg = new DeferredMessage
            {
                piece = currentPiece,
                groupName = groupName,
                senderName = senderName,
                hasOptions = currentPiece.option != null && currentPiece.option.Count > 0,
                storyID = storyID,
                isCompleted = false,
                pieceIndex = i, // 记录片段索引
                totalPieces = pieces.Count // 记录总片段数
            };

            deferredMessages.Add(deferredMsg);

            // 更新解析信息（后续片段可能有不同的发送者）
            ParseMessageInfo(currentPiece, out groupName, out senderName);
        }

        SaveChatData();
        MarkAsUnread(groupName);
        Debug.Log($"已接收延迟剧情: {storyID}，共{pieces.Count}个片段，从{startIndex}开始");
    }

    /// <summary>
    /// 检查是否有指定剧情的未读消息
    /// </summary>
    public bool HasUnreadStory(string storyID)
    {
        return deferredMessages.Any(m => m.storyID == storyID && !m.isCompleted);
    }


    /// <summary>
    /// 处理玩家主动打开聊天时的延迟消息
    /// </summary>
    /// <param name="groupName">要检查的群组名称</param>
    public IEnumerator ProcessDeferredMessagesForGroup(string groupName)
    {
        // 找出该群组的所有延迟消息并按时间排序
        var messagesToProcess = deferredMessages
            .Where(m => m.groupName == groupName)
            .OrderBy(m => m.piece.no)
            .ToList();

        if (messagesToProcess.Count == 0) yield break;

        // 标记为正在处理延迟消息
        isProcessingDeferredMessages = true;

        // 暂停玩家控制
        PlayerController.Instance?.movement?.SetPause(true);

        // 显示聊天界面
        mainPanel.SetActive(true);
        chatPanel.SetActive(true);
        chatGroupNameText.text = groupName;

        foreach (var deferredMsg in messagesToProcess)
        {
            // 添加消息到聊天记录
            AddNewMessageToChat(deferredMsg.piece);
            SaveMessageToHistory(deferredMsg.piece, deferredMsg.groupName, deferredMsg.senderName);
            MarkAsUnread(deferredMsg.groupName);

            // 如果有选项则处理选项
            if (deferredMsg.hasOptions)
            {
                yield return StartCoroutine(ShowOptions(deferredMsg.piece));
            }
            else
            {
                yield return WaitForPlayerContinue();
            }

            // 从延迟列表中移除已处理的消息
            deferredMessages.Remove(deferredMsg);
        }

        // 恢复玩家控制
        PlayerController.Instance?.movement?.SetPause(false);

        // 如果不再处理延迟消息，可以隐藏界面
        if (!isInPhoneMode)
        {
            HideAll();
        }

        isProcessingDeferredMessages = false;
        SaveChatData(); // 保存状态
    }

    // 修改 OpenChatWithGroup 方法
    public void OpenChatWithGroup(string groupName)
    {
        currentChattingGroup = groupName;
        MarkAsRead(groupName);

        chatPanel.SetActive(true);
        //chatNameText.text = groupName;
        chatGroupNameText.text = groupName;

        ClearChatContent();
        LoadChatHistory(groupName);

        // 先显示历史消息，然后处理延迟消息
        StartCoroutine(OpenChatRoutine(groupName));
    }

    private IEnumerator OpenChatRoutine(string groupName)
    {
        // 等待一帧确保UI更新完成
        yield return null;

        // 处理该群的延迟消息
        yield return StartCoroutine(ProcessDeferredMessagesForGroup(groupName));

        ScrollToBottom();
        UpdateNewMessageBadge();
    }

    // public void OpenChatWithGroup(string groupName)
    // {
    //     currentChattingGroup = groupName;
    //     MarkAsRead(groupName);

    //     chatPanel.SetActive(true);
    //     chatNameText.text = groupName;
    //     chatGroupNameText.text = groupName;

    //     ClearChatContent();
    //     LoadChatHistory(groupName);
    //     ScrollToBottom();
    //     UpdateNewMessageBadge();//设置红点
    // }

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
        // 添加任务判断逻辑
        CheckAndHandleTask(piece);
        SaveChatData(); // 每次有新消息都自动保存
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
        SaveChatData(); // 每次有新消息都自动保存
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

    // 更新保存和加载方法
    public void SaveChatData()
    {
        try
        {
            ChatSaveData saveData = new ChatSaveData
            {
                conversations = this.conversations,
                unreadMessages = this.unreadMessages,
                deferredMessages = this.deferredMessages // 新增
            };

            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(chatSavePath, FileMode.Create))
            {
                formatter.Serialize(stream, saveData);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"保存聊天数据失败: {e.Message}");
        }
    }

    private void LoadChatData()
    {
        if (!File.Exists(chatSavePath)) return;

        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(chatSavePath, FileMode.Open))
            {
                ChatSaveData saveData = formatter.Deserialize(stream) as ChatSaveData;
                if (saveData != null)
                {
                    this.conversations = saveData.conversations ?? new Dictionary<string, List<ChatRecord>>();
                    this.unreadMessages = saveData.unreadMessages ?? new Dictionary<string, bool>();
                    this.deferredMessages = saveData.deferredMessages ?? new List<DeferredMessage>();

                    // 迁移旧数据（如果有）
                    foreach (var msg in this.deferredMessages)
                    {
                        if (string.IsNullOrEmpty(msg.storyID))
                        {
                            msg.storyID = "legacy_" + Guid.NewGuid().ToString();
                            msg.isCompleted = false;
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载聊天数据失败: {e.Message}");
        }
    }

    /// <summary>
    /// 检查并处理对话片段中的任务指令
    /// </summary>
    private void CheckAndHandleTask(DialoguePiece piece)
    {
        if (!string.IsNullOrEmpty(piece.task))
        {
            if (piece.task.Contains("接受任务"))
            {
                string pid = piece.task.Replace("接受任务:", "").Trim();
                TaskSystem.Instance.StartTask(pid);
                Debug.Log($"接受任务: {pid}");
            }
            else if (piece.task.Contains("完成任务:"))
            {
                string pid = piece.task.Replace("完成任务:", "").Trim();
                TaskSystem.Instance.CompleteTask(pid);
                Debug.Log($"完成任务: {pid}");
            }
        }
    }

    // // 新增方法：保存聊天数据
    // public void SaveChatData()
    // {
    //     try
    //     {
    //         ChatSaveData saveData = new ChatSaveData
    //         {
    //             conversations = this.conversations,
    //             unreadMessages = this.unreadMessages
    //         };

    //         BinaryFormatter formatter = new BinaryFormatter();
    //         using (FileStream stream = new FileStream(chatSavePath, FileMode.Create))
    //         {
    //             formatter.Serialize(stream, saveData);
    //         }

    //         Debug.Log("聊天数据已保存");
    //     }
    //     catch (System.Exception e)
    //     {
    //         Debug.LogError($"保存聊天数据失败: {e.Message}");
    //     }
    // }

    // // 新增方法：加载聊天数据
    // private void LoadChatData()
    // {
    //     if (!File.Exists(chatSavePath)) return;

    //     try
    //     {
    //         BinaryFormatter formatter = new BinaryFormatter();
    //         using (FileStream stream = new FileStream(chatSavePath, FileMode.Open))
    //         {
    //             ChatSaveData saveData = formatter.Deserialize(stream) as ChatSaveData;
    //             if (saveData != null)
    //             {
    //                 this.conversations = saveData.conversations ?? new Dictionary<string, List<ChatRecord>>();
    //                 this.unreadMessages = saveData.unreadMessages ?? new Dictionary<string, bool>();
    //                 Debug.Log("聊天数据已加载");
    //             }
    //         }
    //     }
    //     catch (System.Exception e)
    //     {
    //         Debug.LogError($"加载聊天数据失败: {e.Message}");
    //         // 加载失败时初始化空数据
    //         this.conversations = new Dictionary<string, List<ChatRecord>>();
    //         this.unreadMessages = new Dictionary<string, bool>();
    //     }
    // }

    // 新增方法：清空存档（用于测试）
    public void ClearChatSave()
    {
        if (File.Exists(chatSavePath))
        {
            File.Delete(chatSavePath);
            Debug.Log("聊天存档已清除");
        }

        // 重置内存中的数据
        conversations.Clear();
        unreadMessages.Clear();
    }
}