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
using SchoolD.Task;

public class ChatSystem : MonoBehaviour, IWindow
{
    public static ChatSystem Instance { get; private set; }
    public bool ShouldPauseTime => false;
    public bool ShouldPausePlayer => true;
    public bool IsOpen => mainPanel.activeSelf;

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
    private List<DeferredMessage> deferredMessages = new List<DeferredMessage>();
    private Dictionary<string, bool> unreadMessages = new Dictionary<string, bool>();
    private string currentChattingGroup;
    private int selectedOptionIndex = -1;
    private bool isInPhoneMode = false;

    [Serializable]
    public class ChatRecord
    {
        public string senderName;
        public string message;
        public string timestamp; // DateTime转为字符串存储
        public bool isPlayerMessage;
    }


    [Serializable]
    public class DeferredMessage
    {
        // 只存储必要字段代替完整piece
        public string dialogueText;
        public List<string> options;
        public string nextIndex;
        public string belongToCSVFileName;

        // 原有字段
        public string groupName;
        public string senderName;
        public bool hasOptions;
        public string storyID;
        public bool isCompleted;
        public int pieceIndex;
        public int totalPieces;

        // 从DialoguePiece提取数据的构造函数
        public DeferredMessage(DialoguePiece piece, string group, string sender, string storyId, int index, int total)
        {
            this.dialogueText = piece.dialogueText;
            this.options = piece.option;
            this.nextIndex = piece.nextIndex;
            this.belongToCSVFileName = piece.belongToCSVFileName;

            this.groupName = group;
            this.senderName = sender;
            this.hasOptions = piece.option != null && piece.option.Count > 0;
            this.storyID = storyId;
            this.isCompleted = false;
            this.pieceIndex = index;
            this.totalPieces = total;
        }
    }
    bool isProcessingDeferredMessages = false;


    [Serializable]
    public class ChatSaveData
    {
        public ChatSerializableDictionary<string, List<ChatRecord>> conversations;
        public ChatSerializableDictionary<string, bool> unreadMessages;
        public List<DeferredMessage> deferredMessages;

        public ChatSaveData()
        {
            conversations = new ChatSerializableDictionary<string, List<ChatRecord>>();
            unreadMessages = new ChatSerializableDictionary<string, bool>();
            deferredMessages = new List<DeferredMessage>();
        }
    }
    // private string chatSavePath;

    private void Awake()
    {
        //chatSavePath = Path.Combine(Application.persistentDataPath, "chat_save.json");
        //Debug.Log("聊天存档路径: " + chatSavePath);
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        //LoadChatData();//加载存档
        Instance = this;
        backButton.onClick.AddListener(() => WindowManager.Instance.CloseWindow(ChatSystem.Instance));

        InitializeUI();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            PrintConversations(); // 按下 P 键时打印会话数据
        }
    }

    private void InitializeUI()
    {
        mainPanel.SetActive(false);
        chatPanel.SetActive(false);
        optionsPanel.SetActive(false);

        newMessageButton.onClick.AddListener(ShowNewMessagesView);
        friendsButton.onClick.AddListener(ShowFriendsView);
    }

    #region Public Interface Methods
    public void Open(params object[] args)
    {
        // 统一通过设置mainPanel.active来管理状态
        mainPanel.SetActive(true);

        // 处理不同打开模式
        bool isPhoneMode = args.Length > 0 && (bool)args[0];
        Debug.LogWarning(isPhoneMode);
        phoneChatPopup.SetActive(isPhoneMode);

        // 默认显示新消息视图
        ShowNewMessagesView();
    }

    public void Close()
    {
        // 关闭所有相关UI
        mainPanel.SetActive(false);
        phoneChatPopup.SetActive(false);
        chatPanel.SetActive(false);
    }
    public void ToggleMainInterface()
    {
        if (IsOpen)
        {
            WindowManager.Instance.CloseWindow(this);
        }
        else
        {
            WindowManager.Instance.OpenWindow(this);
        }
    }

    public void ShowMainInterface()//
    {
        isInPhoneMode = false;
        mainPanel.SetActive(true);
        //phoneChatPopup.SetActive(false);
    }

    public void HideAll()
    {
        mainPanel.SetActive(false);
        //phoneChatPopup.SetActive(false);
    }

    public IEnumerator ShowPhoneMessage(DialoguePiece piece)
    {

        // Setup phone mode
        isInPhoneMode = true;
        WindowManager.Instance.OpenWindow(this, true);
        phoneChatPopup.SetActive(true);

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
            yield return null;
        }

        // Clean up
        if (isInPhoneMode)
        {
            StartCoroutine(WaitForPlayerContinue());
            WindowManager.Instance.CloseWindow(this);
        }
        yield return new WaitUntil(() => !isProcessingDeferredMessages);
    }
    #endregion

    #region 打印函数
    public void PrintConversations()
    {
        if (conversations == null || conversations.Count == 0)
        {
            Debug.Log("没有会话数据。");
            return;
        }

        foreach (var conversation in conversations)
        {
            string groupKey = conversation.Key;
            List<ChatRecord> records = conversation.Value;

            Debug.Log($"会话组: {groupKey}");
            foreach (var record in records)
            {
                string sender = record.senderName;
                string message = record.message;
                string timestamp = record.timestamp;
                bool isPlayerMessage = record.isPlayerMessage;

                string messageInfo = isPlayerMessage ? "（玩家消息）" : "（非玩家消息）";
                Debug.Log($"  发送者: {sender}, 消息: {message}, 时间: {timestamp}, {messageInfo}");
            }
        }
    }

    // 打印未读消息字典
    public void PrintUnreadMessages()
    {
        Debug.LogWarning("=== 未读消息状态 ===");
        if (unreadMessages.Count == 0)
        {
            Debug.LogWarning("无未读消息");
            return;
        }

        foreach (var kvp in unreadMessages)
        {
            Debug.LogWarning($"群组: {kvp.Key}, 未读: {kvp.Value}");
        }
    }

    // 打印延迟消息列表
    public void PrintDeferredMessages()
    {
        Debug.LogWarning("=== 延迟消息列表 ===");
        if (deferredMessages.Count == 0)
        {
            Debug.LogWarning("无延迟消息");
            return;
        }

        foreach (var msg in deferredMessages)
        {
            Debug.LogWarning($"群组: {msg.groupName}\n" +
                     $"发送者: {msg.senderName}\n" +
                     $"剧情ID: {msg.storyID}\n" +
                     $"选项消息: {msg.hasOptions}\n" +
                     $"已完成: {msg.isCompleted}\n" +
                     $"进度: {msg.pieceIndex + 1}/{msg.totalPieces}\n" +
                     "---------------------");
        }
    }

    // 同时打印两者的快捷方法
    public void PrintAllMessageStates()
    {
        Debug.LogWarning("开始打印");
        PrintUnreadMessages();
        PrintDeferredMessages();
    }
    #endregion

    #region 显示中间面板
    public void ShowNewMessagesView()
    {
        // 激活正确的面板
        chatPanel.SetActive(true);
        friendsContainer.gameObject.SetActive(false);
        newMessagesContainer.gameObject.SetActive(true);

        // 清空现有内容
        ClearContainer(newMessagesContainer);

        // 合并所有需要显示的组（来自conversations和deferredMessages）
        var allGroups = new HashSet<string>(conversations.Keys);
        foreach (var msg in deferredMessages)
        {
            allGroups.Add(msg.groupName);
        }

        // 为每个组创建消息项
        foreach (var group in allGroups)
        {
            string displayMessage = null;
            //bool isFromDeferred = false;

            // 1. 优先检查延迟消息（未完成的）
            var deferredMsg = deferredMessages.LastOrDefault(m =>
                m.groupName == group && !m.isCompleted);

            if (deferredMsg != null)
            {
                displayMessage = deferredMsg.dialogueText;
                //isFromDeferred = true;
            }
            // 2. 如果没有延迟消息，检查常规会话
            else if (conversations.TryGetValue(group, out var messages) && messages.Count > 0)
            {
                displayMessage = messages.Last().message;
            }

            // 如果找到有效消息则创建列表项
            if (displayMessage != null)
            {
                CreateMessageListItem(
                    groupName: group,
                    lastMessage: displayMessage,
                    parent: newMessagesContainer
                //isDeferred: isFromDeferred // 可选：用于UI样式区分
                );
            }
        }

        UpdateNewMessageBadge();
    }
    public void ShowFriendsView()
    {
        chatPanel.SetActive(true);
        friendsContainer.gameObject.SetActive(true);
        newMessagesContainer.gameObject.SetActive(false);
        ClearContainer(friendsContainer);

        foreach (var group in conversations.Keys)
        {
            if (conversations[group].Count > 0)
            {
                Debug.Log("好友列表：不为空");
                var lastMessage = conversations[group].Last();
                CreateFriendListItem(group, lastMessage.message, friendsContainer);
            }
        }
    }
    #endregion
    // 在ChatSystem类中添加以下方法

    #region 延迟消息
    /// <summary>
    /// 接收并存储延迟剧情消息
    /// </summary>
    public void ReceiveDeferredStory(string storyID)
    {
        //Debug.LogWarning("接受延迟消息");
        List<DialoguePiece> pieces = DialogueCSVReader.Instance.LoadDialogueData(DialogueLoader.Instance.LoadCSVFromResources(storyID));
        if (pieces == null || pieces.Count == 0)
        {
            Debug.LogWarning($"没有可用的对话片段，剧情ID: {storyID}");
            return;
        }

        // 解析第一个片段的信息获取群组和发送者
        ParseMessageInfo(pieces[0], out string groupName, out string senderName);

        // 确保群组存在（即使没有历史消息）
        if (!conversations.ContainsKey(groupName))
        {
            //Debug.LogWarning("创建新的群组");
            conversations[groupName] = new List<ChatRecord>();
            unreadMessages[groupName] = true;
        }
        bool hasOption = false;
        // 处理所有消息片段
        for (int i = 0; i < pieces.Count; i++)
        {
            // Debug.LogWarning("开始处理消息");
            var piece = pieces[i];
            if (hasOption)
            {
                var deferredMsg = new DeferredMessage(
            piece: piece,
            group: groupName,
            sender: senderName,
            storyId: storyID,
            index: piece.index,
            total: pieces.Count
             );
                deferredMessages.Add(deferredMsg);
            }
            else
            {

                // 1. 普通NPC消息直接存入聊天记录
                if (piece.option == null || piece.option.Count == 0)
                {
                    var npcRecord = new ChatRecord
                    {
                        senderName = senderName,
                        message = piece.dialogueText,
                        timestamp = DateTime.Now.ToString(),
                        isPlayerMessage = false
                    };
                    Debug.LogWarning("是npc消息");
                    conversations[groupName].Add(npcRecord);
                    continue;
                }
                else
                {
                    hasOption = true;
                    // 2. 选项消息特殊处理
                    var deferredMsg = new DeferredMessage(
                 piece: piece,
                 group: groupName,
                 sender: senderName,
                 storyId: storyID,
                 index: piece.no,
                 total: pieces.Count
                  );
                    deferredMessages.Add(deferredMsg);
                }
            }
        }

        //新消息提示
        ChatNewMessageTip.Instance.ShowNewMessageNotification();

        // 更新UI状态
        MarkAsUnread(groupName);
        UpdateNewMessageBadge();
        //SaveChatData();
    }
    #endregion
    // 当需要恢复对话时
    public DialoguePiece ReconstructPiece(DeferredMessage deferred)
    {
        return new DialoguePiece
        {
            dialogueText = deferred.dialogueText,
            option = deferred.options,
            nextIndex = deferred.nextIndex,
            belongToCSVFileName = deferred.belongToCSVFileName
        };
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
        // 获取待处理消息（自动排除已完成的）
        var messagesToProcess = deferredMessages
            .Where(m => m.groupName == groupName && !m.isCompleted)
            .OrderBy(m => m.pieceIndex)
            .ToList();

        if (messagesToProcess.Count == 0) yield break;

        isProcessingDeferredMessages = true;

        foreach (var deferredMsg in messagesToProcess.ToArray()) // 使用ToArray避免修改集合问题
        {
            //Debug.Log("进入后序处理");
            var piece = new DialoguePiece
            {
                dialogueText = deferredMsg.dialogueText,      // 消息文本
                option = deferredMsg.options,                 // 选项列表（如果有）
                nextIndex = deferredMsg.nextIndex,            // 后续消息索引（如 "25|26"）
                belongToCSVFileName = deferredMsg.belongToCSVFileName, // 所属CSV文件名
                name = deferredMsg.senderName,                // 发送者名称
                onLeft = false,                               // 默认显示在右侧（玩家在右）
            };


            AddNewMessageToChat(piece);
            SaveMessageToHistory(piece, groupName, deferredMsg.senderName);

            if (deferredMsg.hasOptions)
            {
                // 遇到选项就交给ProcessPendingOptions处理
                yield return StartCoroutine(ProcessPendingOptions(groupName));
                yield break; // 退出当前循环，由选项处理接管
            }
            else
            {
                deferredMsg.isCompleted = true;
                deferredMessages.Remove(deferredMsg);
            }
            yield return new WaitForSeconds(0.5f);//等待0.5后，继续发送下一条
        }

        isProcessingDeferredMessages = false;
        MarkAsRead(groupName);
        //SaveChatData();
    }

    // 修改 OpenChatWithGroup 方法
    public void OpenChatWithGroup(string groupName)
    {
        // 确保主面板保持激活
        if (!mainPanel.activeSelf)
        {
            mainPanel.SetActive(true);
        }

        // 如果群组不存在，初始化空列表
        if (!conversations.ContainsKey(groupName))
        {
            conversations[groupName] = new List<ChatRecord>();
            unreadMessages[groupName] = false;
        }

        currentChattingGroup = groupName;
        MarkAsRead(groupName);

        // 修改这里：不通过WindowManager直接控制UI状态
        chatPanel.SetActive(true);
        chatGroupNameText.text = groupName;
        ClearChatContent();

        // 加载历史消息
        LoadChatHistory(groupName);

        // 延迟一帧再处理选项，确保UI完全初始化
        StartCoroutine(DelayedProcessOptions(groupName));
    }
    #region 选项函数
    private IEnumerator DelayedProcessOptions(string groupName)
    {
        yield return null; // 等待一帧确保UI完成初始化
        yield return StartCoroutine(ProcessPendingOptions(groupName));
    }

    private IEnumerator ProcessPendingOptions(string groupName)
    {
        var pendingOption = deferredMessages.FirstOrDefault(m =>
            m.groupName == groupName && !m.isCompleted && m.hasOptions);

        if (pendingOption == null) yield break;

        // UI激活逻辑
        if (!chatPanel.activeSelf) chatPanel.SetActive(true);
        mainPanel.SetActive(true);

        var optionPiece = new DialoguePiece
        {
            dialogueText = pendingOption.dialogueText,
            option = pendingOption.options,
            nextIndex = pendingOption.nextIndex,
            belongToCSVFileName = pendingOption.belongToCSVFileName,
            name = pendingOption.senderName
        };

        yield return StartCoroutine(ShowOptions(optionPiece));


        if (selectedOptionIndex != -1)
        {
            // 处理选项结果
            ProcessOptionResult(optionPiece);

            // 标记完成但先不移除
            pendingOption.isCompleted = true;
            //SaveChatData();

            // 直接继续处理后续消息
            yield return StartCoroutine(ProcessDeferredMessagesForGroup(groupName));
        }

        // 最后再移除已处理的选项
        deferredMessages.Remove(pendingOption);
        ScrollToBottom();
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator ShowOptions(DialoguePiece piece)
    {
        yield return new WaitForSeconds(optionDisplayDelay);

        ClearOptions();
        var optionButtons = CreateOptionButtons(piece);

        if (optionButtons.Count == 0)
        {
            selectedOptionIndex = 0;
            yield break;
        }

        // 确保父面板激活
        if (!optionsPanel.transform.parent.gameObject.activeSelf)
        {
            optionsPanel.transform.parent.gameObject.SetActive(true);
        }

        optionsPanel.SetActive(true);
        selectedOptionIndex = -1;


        yield return new WaitUntil(() => selectedOptionIndex != -1);

        Debug.Log("当前消息选择：" + selectedOptionIndex);
        optionsPanel.SetActive(false);
        CleanupOptions(optionButtons);
    }
    private void ProcessOptionResult(DialoguePiece piece)
    {
        // 记录玩家选择
        var optionPiece = new DialoguePiece
        {
            name = Settings.playerName,
            dialogueText = piece.option[selectedOptionIndex],
            onLeft = false
        };
        AddNewMessageToChat(optionPiece);
        SaveMessageToHistory(optionPiece, currentChattingGroup, Settings.playerName);

        // 处理后续消息
        if (!string.IsNullOrEmpty(piece.nextIndex))
        {
            string[] nextIndices = piece.nextIndex.Split('|');
            // 移除所有未被选中的选项的延迟消息
            for (int i = 0; i < nextIndices.Length; i++)
            {
                if (i != selectedOptionIndex) // 如果不是当前选中的选项
                {
                    string otherIndex = nextIndices[i];
                    deferredMessages.RemoveAll(msg =>
                        msg.groupName == currentChattingGroup &&
                        msg.pieceIndex.ToString() == otherIndex);
                }
            }
        }
        // 打印删除后的延迟消息
        Debug.Log("删除后的延迟消息列表:");
        foreach (var msg in deferredMessages)
        {
            Debug.Log($"- 群组: {msg.groupName}, 索引: {msg.pieceIndex}, 是否完成: {msg.isCompleted}");
        }
    }
    #endregion
    private IEnumerator OpenChatRoutine(string groupName)
    {
        // 等待一帧确保UI更新完成
        //yield return null;

        // 处理该群的延迟消息
        yield return StartCoroutine(ProcessDeferredMessagesForGroup(groupName));

        ScrollToBottom();
        UpdateNewMessageBadge();
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
            if (groupName.Equals("NQChat"))
                groupName = senderName;

            // 调试输出
            Debug.Log($"原始字符串: {piece.name}");
            Debug.Log($"匹配结果: 群组='{groupName}', 发送者='{senderName}'");
        }
        else
        {
            senderName = piece.name;
        }

        //Debug.LogWarning($"无法解析消息格式: {piece.name}");

    }

    private void AddNewMessageToChat(DialoguePiece piece)
    {
        ParseMessageInfo(piece, out string groupName, out string senderName);

        bool isPlayer = senderName == Settings.playerName;

        var newRecord = new ChatRecord
        {
            senderName = senderName,
            message = piece.dialogueText,
            timestamp = TimeManager.Instance.GetCurrentDate().ToString("MM-dd HH:mm"),
            isPlayerMessage = isPlayer
        };

        AddMessageToChat(newRecord, GetPreviousMessage(groupName));
        ScrollToBottom();
    }

    private void AddMessageToChat(ChatRecord record, ChatRecord previousRecord = null)
    {
        // 检查是否需要显示时间标签（第一条消息或间隔≥30分钟）
        if (ShouldDisplayTimeLabel(record.timestamp, previousRecord?.timestamp))
        {
            CreateTimeLabel(record.timestamp);
        }

        // 创建消息UI（剩余部分保持不变）
        GameObject prefab = record.isPlayerMessage ? playerMessagePrefab : messagePrefab;
        var messageObj = Instantiate(prefab, chatContent);
        var messageItem = messageObj.GetComponent<ChatMessageItem>();

        if (messageItem != null)
        {
            messageItem.Initialize(record.senderName, record.message, !record.isPlayerMessage);

            Sprite avatar = DialogueCSVReader.Instance.GetAvatarForSender(record.senderName);
            if (avatar != null)
            {
                messageItem.SetAvatar(avatar);
            }
        }

    }

    // 新增方法：判断是否需要显示时间标签
    private bool ShouldDisplayTimeLabel(string currentTime, string previousTime)
    {
        // 第一条消息总是显示时间
        if (string.IsNullOrEmpty(previousTime))
            return true;

        try
        {
            // 解析字符串时间为DateTime（假设格式为"yyyy-MM-dd HH:mm:ss"）
            DateTime current = DateTime.Parse(currentTime);
            DateTime previous = DateTime.Parse(previousTime);

            // 计算时间差（分钟）
            double minutesDiff = (current - previous).TotalMinutes;
            return minutesDiff >= 30;
        }
        catch
        {
            // 如果时间格式解析失败，默认显示时间标签
            Debug.LogWarning($"时间格式解析失败: current={currentTime}, previous={previousTime}");
            return true;
        }
    }

    private void CreateTimeLabel(string time)
    {
        var timeLabel = Instantiate(timeLabelPrefab, chatContent);
        var timeLabelItem = timeLabel.GetComponent<ChatMessageItem>();
        timeLabelItem.InitializeAsTimeLabel(time);
    }

    private void SaveMessageToHistory(DialoguePiece piece, string groupName, string senderName)
    {
        bool isPlayer = senderName == Settings.playerName;

        var newRecord = new ChatRecord
        {
            senderName = senderName,
            message = piece.dialogueText,
            timestamp = DateTime.Now.ToString(),
            isPlayerMessage = isPlayer
        };

        if (!conversations.ContainsKey(groupName))
        {
            conversations[groupName] = new List<ChatRecord>();
        }
        conversations[groupName].Add(newRecord);
        // 添加任务判断逻辑
        CheckAndHandleTask(piece);
        // SaveChatData(); // 每次有新消息都自动保存
    }

    private ChatRecord GetPreviousMessage(string groupName)
    {
        if (conversations.TryGetValue(groupName, out var messages) && messages.Count >= 2)
        {
            return messages[messages.Count - 2];
        }
        return null;
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
            Debug.Log("创建成功");
            bool isUnread = unreadMessages.TryGetValue(groupName, out bool unread) && unread;
            bool hasOptions = deferredMessages.Any(m => m.groupName == groupName && !m.isCompleted);

            item.Initialize(
                groupName,
                lastMessage,
                isUnread || hasOptions, // 有未读消息或待处理选项都显示红点
                () => OpenChatWithGroup(groupName)
            );

            if (DialogueCSVReader.Instance.GetAvatarForSender(groupName) != null)
            {
                item.SetAvatar(DialogueCSVReader.Instance.GetAvatarForSender(groupName));
            }
            itemObj.SetActive(true);
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
            if (DialogueCSVReader.Instance.GetAvatarForSender(groupName) != null)
            {
                item.SetAvatar(DialogueCSVReader.Instance.GetAvatarForSender(groupName));
            }
            itemObj.SetActive(true);
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
        //SaveChatData(); // 每次有新消息都自动保存
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


    public void SaveChatData(string chatSavePath)
    {
        try
        {
            var data = new ChatSaveData();

            // 转换普通字典到可序列化字典
            data.conversations.FromDictionary(conversations);
            data.unreadMessages.FromDictionary(unreadMessages);
            data.deferredMessages = new List<DeferredMessage>(deferredMessages);

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(chatSavePath, json);
            // Debug.Log($"存档成功，路径: {chatSavePath}\n内容: {json}");
        }
        catch (Exception e)
        {
            Debug.LogError($"存档失败: {e.Message}\n{e.StackTrace}");
        }
    }

    public bool LoadChatData(string chatSavePath)
    {
        if (!File.Exists(chatSavePath))
        {
            InitializeNewData();
            return false;
        }

        try
        {
            string json = File.ReadAllText(chatSavePath);
            Debug.Log($"读取存档内容: {json}");

            var data = JsonUtility.FromJson<ChatSaveData>(json);

            conversations = data.conversations?.ToDictionary() ?? new Dictionary<string, List<ChatRecord>>();
            unreadMessages = data.unreadMessages?.ToDictionary() ?? new Dictionary<string, bool>();
            deferredMessages = data.deferredMessages ?? new List<DeferredMessage>();

            Debug.Log($"加载成功: {conversations.Count}个会话，{deferredMessages.Count}条待处理消息");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"读档失败: {e.Message}\n{e.StackTrace}");
            InitializeNewData();
            return false;
        }
    }

    private void InitializeNewData()
    {
        conversations = new Dictionary<string, List<ChatRecord>>();
        unreadMessages = new Dictionary<string, bool>();
        deferredMessages = new List<DeferredMessage>();
        Debug.Log("初始化新的空数据");
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


    private void InitializeDefaultData()
    {
        conversations = new Dictionary<string, List<ChatRecord>>();
        unreadMessages = new Dictionary<string, bool>();
        deferredMessages = new List<DeferredMessage>();
    }

    // // 新增方法：清空存档（用于测试）
    // public void ClearChatSave()
    // {
    //     if (File.Exists(chatSavePath))
    //     {
    //         File.Delete(chatSavePath);
    //         Debug.Log("聊天存档已清除");
    //     }

    //     // 重置内存中的数据
    //     conversations.Clear();
    //     unreadMessages.Clear();
    // }
}