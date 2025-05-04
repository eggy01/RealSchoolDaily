using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReplyItem : MonoBehaviour
{
    // UI元素引用
    public Image avatarImage;            // 用户头像
    public TextMeshProUGUI usernameText; // 用户名
    public Image levelImage;             // 用户等级图标
    public TextMeshProUGUI floorText;    // 楼层号
    public TextMeshProUGUI contentText;  // 回复内容
    public GameObject replyToPanel;      // 引用回复面板
    public TextMeshProUGUI replyToText;  // 引用回复内容
    public TextMeshProUGUI replyToName;
    public Image backgroundImage;       // 背景图片

    public Button likeButton;          // 点赞按钮
    public Button dislikeButton;       // 点踩按钮
    public Sprite likeNormal;          // 点赞按钮默认图标
    public Sprite likeSelected;        // 点赞按钮选中图标
    public Sprite dislikeNormal;       // 点踩按钮默认图标
    public Sprite dislikeSelected;     // 点踩按钮选中图标

    private Reply currentReply;        // 当前回复数据
    private ForumUIManager uiManager;  // 论坛UI管理器引用

    // 初始化回复项
    public void Initialize(Reply reply, ForumUIManager manager, int floorNumber)
    {
        currentReply = reply;
        uiManager = manager;

        // 设置背景
        if (floorNumber % 2 == 1)
            backgroundImage.sprite = manager.oddBackgroundSprite;
        else
            backgroundImage.sprite = manager.evenBackgroundSprite;

        // 设置头像
        if (!string.IsNullOrEmpty(reply.avatar))
        {
            Sprite loadedAvatar = Resources.Load<Sprite>(reply.avatar);
            if (loadedAvatar != null)
            {
                avatarImage.sprite = loadedAvatar;
            }
            else
            {
                avatarImage.sprite = uiManager.defaultAvatar;
            }
        }
        else
        {
            avatarImage.sprite = uiManager.defaultAvatar;
        }

        // 设置楼层号
        floorText.text = reply.floor;
        // 设置回复内容
        contentText.text = reply.content;

        // 设置用户名（如果为空则生成随机数字用户名）
        usernameText.text = string.IsNullOrEmpty(reply.username) ? UnityEngine.Random.Range(1000000, 9999999).ToString() : reply.username;

        // 设置用户等级图标
        if (!string.IsNullOrEmpty(reply.level))
        {
            int levelIndex = int.Parse(reply.level) - 1; // 转换等级为数组索引（假设等级是1-6的字符串）
            if (levelIndex >= 0 && levelIndex < uiManager.Level.Length)
            {
                levelImage.sprite = uiManager.Level[levelIndex]; // 设置等级图标
                levelImage.gameObject.SetActive(true);           // 显示等级图标
            }
        }

        // 处理回复引用
        if (!string.IsNullOrEmpty(reply.replyTo))
        {
            Reply targetReply = FindTargetReply(reply.replyTo); // 查找被引用的回复
            if (targetReply != null)
            {
                replyToPanel.SetActive(true);   // 显示引用面板
                replyToName.text = $"{targetReply.floor}{targetReply.username}";
                replyToText.text = $"{targetReply.content}"; // 设置引用内容
            }
        }

        // 初始化点赞系统
        LoadLikeState();                        // 加载点赞状态
        likeButton.onClick.AddListener(ToggleLike);     // 添加点赞按钮点击事件
        dislikeButton.onClick.AddListener(ToggleDislike);// 添加点踩按钮点击事件
    }

    // 查找特定楼层的回复
    Reply FindTargetReply(string floor)
    {
        foreach (Reply reply in uiManager.currentPost.replies)
        {
            if (reply.floor == floor)
            {
                return reply; // 返回匹配楼层的回复
            }
        }
        return null; // 未找到返回空
    }

    // 切换点赞状态
    void ToggleLike()
    {
        bool newState = !GetLikeState();       // 获取新的点赞状态
        SetLikeState(newState);               // 设置点赞状态
        if (newState) SetDislikeState(false); // 如果点赞，则取消点踩
        UpdateButtonAppearance();             // 更新按钮外观
    }

    // 切换点踩状态
    void ToggleDislike()
    {
        bool newState = !GetDislikeState();    // 获取新的点踩状态
        SetDislikeState(newState);            // 设置点踩状态
        if (newState) SetLikeState(false);    // 如果点踩，则取消点赞
        UpdateButtonAppearance();             // 更新按钮外观
    }

    // 更新按钮外观
    void UpdateButtonAppearance()
    {
        // 根据状态更新按钮图标
        likeButton.image.sprite = GetLikeState() ? likeSelected : likeNormal;
        dislikeButton.image.sprite = GetDislikeState() ? dislikeSelected : dislikeNormal;
    }

    // 获取存储键（用于PlayerPrefs）
    string GetStorageKey()
    {
        return $"{uiManager.currentPost.postTitle}_{currentReply.floor}"; // 生成唯一存储键
    }

    // 获取点赞状态
    bool GetLikeState()
    {
        return PlayerPrefs.GetInt(GetStorageKey() + "_like", 0) == 1; // 从PlayerPrefs读取点赞状态
    }

    // 获取点踩状态
    bool GetDislikeState()
    {
        return PlayerPrefs.GetInt(GetStorageKey() + "_dislike", 0) == 1; // 从PlayerPrefs读取点踩状态
    }

    // 设置点赞状态
    void SetLikeState(bool state)
    {
        PlayerPrefs.SetInt(GetStorageKey() + "_like", state ? 1 : 0); // 将点赞状态保存到PlayerPrefs
    }

    // 设置点踩状态
    void SetDislikeState(bool state)
    {
        PlayerPrefs.SetInt(GetStorageKey() + "_dislike", state ? 1 : 0); // 将点踩状态保存到PlayerPrefs
    }

    // 加载点赞状态
    void LoadLikeState()
    {
        UpdateButtonAppearance(); // 根据保存的状态更新按钮外观
    }
}