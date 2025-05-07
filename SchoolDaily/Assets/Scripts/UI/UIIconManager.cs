using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIIconManager : MonoBehaviour
{
    #region 单例
    public static UIIconManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    #endregion

    public Button forum;
    public Button phone;
    public TextMeshProUGUI Gold;
    void Start()
    {
        forum.onClick.AddListener(() => WindowManager.Instance.OpenWindow(ForumUIManager.Instance));
        phone.onClick.AddListener(() => WindowManager.Instance.OpenWindow(ChatSystem.Instance, true));
    }

    public void GoldOut()
    {
        int currentGold = PlayerInformation.Instance.CurrentData.gold;
        Gold.text = $"{currentGold}";
    }
}
