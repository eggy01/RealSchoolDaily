using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.Events;

[System.Serializable]
public class GoldUpdateEvent : UnityEvent<int> { }

public class PlayerInformation : MonoBehaviour
{
    #region 单例模式
    public static PlayerInformation Instance { get; private set; }
    #endregion

    [Header("UI")]
    public TextMeshProUGUI PlayerNameUI;
    public TextMeshProUGUI PlayerLifeUI;
    public TextMeshProUGUI PlayerStrengthUI;
    public TextMeshProUGUI PlayerMoodUI;
    public TextMeshProUGUI PlayerMuddlednessUI;
    public TextMeshProUGUI GoldUI;
    public TextMeshProUGUI PlayerWarehouseUI;

    #region Unity生命周期
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        UIIconManager.Instance.GoldOut();
        UpdateUI();
    }
    #endregion

    #region 数据管理
    public PlayerData CurrentData
    {
        get => SaveManager.Instance.GetTempData().playerData;
        set => SaveManager.Instance.GetTempData().playerData = value;
    }

    // 当切换存档槽位时调用
    public void RefreshFromSaveData()
    {
        UIIconManager.Instance.GoldOut();
        UpdateUI();
    }
    #endregion

    #region 金币管理
    public int CurrentGold => CurrentData.gold;

    public bool TrySpendGold(int amount)
    {
        if (amount <= 0 || CurrentGold < amount) return false;

        CurrentData.gold = Mathf.Max(0, CurrentGold - amount);
        UIIconManager.Instance.GoldOut();
        UpdateUI();
        return true;
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        CurrentData.gold += amount;
        UIIconManager.Instance.GoldOut();
        UpdateUI();
    }
    #endregion

    #region 属性管理
    // 增加玩家生命值上限
    public void AddLife(int value)
    {
        CurrentData.life += value;
        UpdateUI();
    }
    //在别的地方调用
    //playerController.AddLife(10);

    // 减少玩家生命上限
    public void SubtractLife(int value)
    {
        CurrentData.life -= value;
        UpdateUI();
    }

    // 增加玩家体力上限
    public void AddStrength(int value)
    {
        CurrentData.strength += value;
        UpdateUI();
    }

    // 减少玩家体力上限
    public void SubtractStrength(int value)
    {
        CurrentData.strength -= value;
        UpdateUI();
    }

    // 增加玩家心情
    public void AddMood(int value)
    {
        CurrentData.mood = Mathf.Clamp(CurrentData.mood + value, 0, 100);
        UpdateUI();
    }

    // 减少玩家心情
    public void SubtractMood(int value)
    {
        CurrentData.mood = Mathf.Clamp(CurrentData.mood - value, 0, 100);
        UpdateUI();
    }

    // 增加玩家失序值
    public void AddMuddledness(int value)
    {
        CurrentData.muddledness = Mathf.Clamp(CurrentData.muddledness + value, 0, 100);
        UpdateUI();
    }

    // 减少玩家失序值
    public void SubtractMuddledness(int value)
    {
        CurrentData.muddledness = Mathf.Clamp(CurrentData.muddledness - value, 0, 100);
        UpdateUI();
    }

    // 增加声望
    public void AddFame(int value)
    {
        CurrentData.fame = Mathf.Max(CurrentData.fame + value, 0);
        UpdateUI();
    }

    public void SubtractFame(int value)
    {
        CurrentData.fame = Mathf.Max(CurrentData.fame - value, 0);
        UpdateUI();
    }

    // 道德
    public void AddMorality(int value)
    {
        CurrentData.morality = Mathf.Min(CurrentData.morality + value, 100);
        UpdateUI();
    }

    public void SubtractMorality(int value)
    {
        CurrentData.morality -= value; // 允许负数
        UpdateUI();
    }

    // 智力
    public void AddIntelligence(int value)
    {
        CurrentData.intelligence = Mathf.Clamp(CurrentData.intelligence + value, 0, 100);
        UpdateUI();
    }

    public void SubtractIntelligence(int value)
    {
        CurrentData.intelligence = Mathf.Clamp(CurrentData.intelligence - value, 0, 100);
        UpdateUI();
    }

    // 理解力
    public void AddComprehension(int value)
    {
        CurrentData.comprehension = Mathf.Clamp(CurrentData.comprehension - value, 0, 100);
        UpdateUI();
    }

    public void SubtractComprehension(int value)
    {
        CurrentData.comprehension = Mathf.Clamp(CurrentData.comprehension - value, 0, 100);
        UpdateUI();
    }

    // 天赋
    public void AddTalent(int value)
    {
        CurrentData.talent = Mathf.Clamp(CurrentData.talent + value, 0, 100);
        UpdateUI();
    }

    public void SubtractTalent(int value)
    {
        CurrentData.talent = Mathf.Clamp(CurrentData.talent - value, 0, 100);
        UpdateUI();
    }

    // 社交
    public void AddSociety(int value)
    {
        CurrentData.society = Mathf.Clamp(CurrentData.society + value, 0, 100);
        UpdateUI();
    }

    public void SubtractSociety(int value)
    {
        CurrentData.society = Mathf.Clamp(CurrentData.society - value, 0, 100);
        UpdateUI();
    }

    // 增加玩家仓库容量
    public void AddWarehouse(int value)
    {
        CurrentData.warehouse += value;
        UpdateUI();
    }

    // 减少玩家仓库容量
    public void SubtractWarehouse(int value)
    {
        CurrentData.warehouse -= value;
        UpdateUI();
    }
    #endregion

    #region 更新UI

    private void UpdateUI()
    {
        PlayerNameUI.text = "姓名: " + CurrentData.name;
        PlayerLifeUI.text = "生命上限: " + CurrentData.life;
        PlayerStrengthUI.text = "体力上限: " + CurrentData.strength;
        PlayerMoodUI.text = "当前心情: " + CurrentData.mood;
        PlayerMuddlednessUI.text = "失序值: " + CurrentData.muddledness;
        PlayerWarehouseUI.text = "库容量: " + CurrentData.warehouse;
         GoldUI.text = "低保: ￥" + CurrentData.gold;
    }
    #endregion
}