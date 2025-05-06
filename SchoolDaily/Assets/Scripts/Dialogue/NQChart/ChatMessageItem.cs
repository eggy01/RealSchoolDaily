using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class ChatMessageItem : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image avatarImage;
    [SerializeField] private TextMeshProUGUI senderText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private RectTransform bubbleTransform;
    [SerializeField] private Image bubbleImage;
    [SerializeField] private GameObject timeLabelObject;


    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        timeLabelObject?.SetActive(false);
    }

    // 初始化普通消息
    public void Initialize(string sender, string message, bool isLeftSide)
    {
        Debug.Log("创建消息列表");
        // 基础信息设置
        if (senderText != null) senderText.text = sender;
        if (messageText != null) messageText.text = message;

        // 默认隐藏时间标签
        if (timeLabelObject != null)
        {
            timeLabelObject.SetActive(false);
        }
    }

    // 初始化时间标签
    public void InitializeAsTimeLabel(string timeString)
    {
        if (timeLabelObject != null)
        {
            // 隐藏其他元素
            if (bubbleTransform != null) bubbleTransform.gameObject.SetActive(false);
            if (avatarImage != null) avatarImage.gameObject.SetActive(false);
            Debug.Log("初始化时间");
            // 显示时间标签
            timeLabelObject.SetActive(true);
            var timeText = timeLabelObject.GetComponentInChildren<TextMeshProUGUI>();
            if (timeText != null) timeText.text = timeString;
        }
    }

    public void SetAvatar(Sprite avatar)
    {
        if (avatarImage != null)
        {
            avatarImage.sprite = avatar;
            avatarImage.gameObject.SetActive(avatar != null);
        }
    }

    public void SetClickAction(System.Action onClick)
    {
        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onClick?.Invoke());
        }
    }
}