using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using TMPro;

public class ForumUIManager : MonoBehaviour, IWindow
{
    public static ForumUIManager Instance;
    private bool _isForumOpen;
    public bool IsOpen => _isForumOpen;
    public bool ShouldPauseTime => false;
    public bool ShouldPausePlayer => true;

    #region UI配置
    [Header("论坛面板")]
    public GameObject forumPanel; // 论坛
    public GameObject UIPanel;

    [Header("UI组件")]
    public Button close;
    public Transform sectionListParent; // 导航栏
    public GameObject sectionButtonPrefab; // 版块按钮预制体

    public GameObject postListPanel; // 帖子列表页
    public Transform postListContent; // 帖子列表内容区域
    public GameObject postEntryPrefab; // 帖子条目预制体

    public GameObject postDetailPanel; // 详情页
    public Transform replyContent; // 回复内容区域
    public GameObject replyPrefab; // 回复预制体
    public Button backButton;      // 返回按钮

    [Header("版块按钮样式")]
    public Sprite selectedSectionSprite; // 选中状态的版块按钮图片
    public Sprite normalSectionSprite;   // 普通状态的版块按钮图片

    [Header("回复背景样式")]
    public Sprite oddBackgroundSprite;  // 单数楼层背景
    public Sprite evenBackgroundSprite; // 双数楼层背景

    private Image selectedSectionButton; // 当前选中的版块按钮

    // 数据
    private ForumData forumData; // 论坛数据
    public Post currentPost; // 当前显示的帖子

    // 配置参数
    public Sprite defaultAvatar; // 默认头像

    public Sprite[] Level = new Sprite[6];

    #endregion

    #region Unity生命周期
    void Start()
    {
        forumPanel.SetActive(false);
        LoadData(); // 加载论坛数据
        GenerateSectionList(); // 生成版块列表
    }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        close.onClick.AddListener(() => WindowManager.Instance.CloseWindow(this));
    }

    // 从Resources文件夹加载论坛数据
    void LoadData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Forum"); // 加载名为"Forum"的文本资源
        forumData = JsonUtility.FromJson<ForumData>(jsonFile.text); // 将JSON文本转换为ForumData对象
    }
    #endregion

    #region 打开关闭论坛
    public void Open(params object[] args)
    {
        OpenForum();
        _isForumOpen = true;
    }

    public void Close()
    {
        CloseForum();
        _isForumOpen = false;
    }
    public void OpenForum()
    {
        UIPanel.SetActive(false);
        forumPanel.SetActive(true);
        ResetAllSectionButtons();
        ShowDefaultSection();
        postListPanel.SetActive(true);
        postDetailPanel.SetActive(false);
    }

    public void CloseForum()
    {
        UIPanel.SetActive(true);
        forumPanel.SetActive(false);
    }
    #endregion

    #region 生成版块列表
    void GenerateSectionList()
    {
        foreach (Section section in forumData.sections)
        {
            GameObject btn = Instantiate(sectionButtonPrefab, sectionListParent);
            TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = section.sectionName;

            Button btnComp = btn.GetComponent<Button>();
            Image btnImage = btn.GetComponent<Image>();
            btnImage.sprite = normalSectionSprite;

            btnComp.onClick.AddListener(() =>
            {
                // 切换按钮状态
                if (selectedSectionButton != null)
                    selectedSectionButton.sprite = normalSectionSprite;

                selectedSectionButton = btnImage;
                btnImage.sprite = selectedSectionSprite;

                ShowPosts(section.posts);
            });
        }
    }
    #endregion

    #region  显示帖子列表
    void ShowPosts(List<Post> posts)
    {
        postListPanel.SetActive(true); // 显示帖子列表面板
        postDetailPanel.SetActive(false); // 隐藏帖子详情面板

        ClearChildren(postListContent); // 清空帖子列表内容区域

        foreach (Post post in posts)
        { // 遍历所有帖子
            if (ShouldShowPost(post.postTime))
            { // 判断帖子是否应该显示
                GameObject entry = Instantiate(postEntryPrefab, postListContent); // 创建帖子条目
                entry.GetComponentInChildren<TextMeshProUGUI>().text = post.postTitle; // 设置帖子标题
                entry.GetComponent<Button>().onClick.AddListener(() => ShowPostDetail(post)); // 添加点击事件，显示帖子详情

                // 获取hot图片组件
                Image hotIndicator = entry.transform.Find("Hot").GetComponent<Image>();

                // 如果回复数量超过10条，显示hot图片
                if (post.replies.Count >= 10)
                {
                    hotIndicator.enabled = true;
                }
                else
                {
                    hotIndicator.enabled = false;
                }
            }
        }
    }

    // 判断帖子是否应该显示
    bool ShouldShowPost(string postTime)
    {
        if (string.IsNullOrEmpty(postTime)) return true; // 如果时间为空，显示帖子

        DateTime postDate;
        if (DateTime.TryParse(postTime, out postDate))
        { // 尝试解析时间
            return DateTime.Now >= postDate; // 如果当前时间晚于帖子时间，显示帖子
        }
        return true; // 解析失败时显示帖子
    }

    // 显示默认版块
    private void ResetAllSectionButtons()
    {
        foreach (Transform child in sectionListParent)
        {
            child.GetComponent<Image>().sprite = normalSectionSprite;
        }
    }
    void ShowDefaultSection()
    {
        if (forumData.sections.Count > 0)
        {
            var firstSection = forumData.sections[0];
            ShowPosts(firstSection.posts);

            // 设置第一个按钮的选中状态
            Transform firstButton = sectionListParent.GetChild(0);
            selectedSectionButton = firstButton.GetComponent<Image>();
            selectedSectionButton.sprite = selectedSectionSprite;
        }
    }

    // 返回帖子列表
    public void ShowPostList()
    {
        postDetailPanel.SetActive(false);
        postListPanel.SetActive(true);

        // 保持当前版块的选中状态
        if (selectedSectionButton != null)
            ShowPosts(GetCurrentSectionPosts());
    }

    // 获取当前选中版块的帖子列表
    private List<Post> GetCurrentSectionPosts()
    {
        foreach (Transform sectionBtn in sectionListParent)
        {
            if (sectionBtn.GetComponent<Image>() == selectedSectionButton)
            {
                int index = sectionBtn.GetSiblingIndex();
                return forumData.sections[index].posts;
            }
        }
        return forumData.sections[0].posts;
    }
    #endregion

    #region  显示帖子详情
    void ShowPostDetail(Post post)
    {
        currentPost = post; // 设置当前帖子
        postListPanel.SetActive(false); // 隐藏帖子列表面板
        postDetailPanel.SetActive(true); // 显示帖子详情面板

        ClearChildren(replyContent); // 清空回复内容区域
        backButton.onClick.AddListener(ShowPostList); // 添加返回按钮点击事件

        // 回复项初始化
        for (int i = 0; i < post.replies.Count; i++)
        {
            GameObject replyObj = Instantiate(replyPrefab, replyContent);
            var replyItem = replyObj.GetComponent<ReplyItem>();
            replyItem.Initialize(post.replies[i], this, i + 1); // 传入楼层序号
        }

    }
    #endregion

    // 清空指定父对象的所有子对象
    void ClearChildren(Transform parent)
    {
        foreach (Transform child in parent)
        { // 遍历所有子对象
            Destroy(child.gameObject); // 销毁子对象
        }
    }
}