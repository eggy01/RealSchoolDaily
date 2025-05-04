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

    [Header("Data")]
    public PlayerData playerData;

    [Header("UI")]
    public TextMeshProUGUI PlayerNameUI;
    public TextMeshProUGUI PlayerLifeUI;
    public TextMeshProUGUI PlayerStrengthUI;
    public TextMeshProUGUI PlayerMoodUI;
    public TextMeshProUGUI PlayerMuddlednessUI;
    public TextMeshProUGUI GoldUI;
    public TextMeshProUGUI PlayerWarehouseUI;

    [Header("事件")]
    public GoldUpdateEvent OnGoldUpdated;

    private string savePath = "PlayerData.json";

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

        LoadPlayerData();
        InitializeData();
    }

    private void Start()
    {
        OnGoldUpdated.AddListener(UpdateGoldUI);
        UpdateUI();
    }

    private void OnDestroy()
    {
        OnGoldUpdated.RemoveListener(UpdateGoldUI);
    }
    #endregion

    #region 数据管理
    private void InitializeData()
    {
        if (string.IsNullOrEmpty(playerData.name))
        {
            playerData = new PlayerData();
        }
    }

    public void SavePlayerData()
    {
        string jsonPlayerData = JsonUtility.ToJson(playerData);
        File.WriteAllText(savePath, jsonPlayerData);
    }

    private void LoadPlayerData()
    {
        if (File.Exists(savePath))
        {
            string jsonPlayerData = File.ReadAllText(savePath);
            playerData = JsonUtility.FromJson<PlayerData>(jsonPlayerData);
        }
    }

    private void SaveAndUpdate()
    {
        SavePlayerData();
        OnGoldUpdated?.Invoke(CurrentGold);
        UpdateUI();
    }
    #endregion

    #region 金币管理
    public int CurrentGold => playerData.gold;

    public bool TrySpendGold(int amount)
    {
        if (amount <= 0 || CurrentGold < amount) return false;

        playerData.gold = Mathf.Max(0, CurrentGold - amount);
        SaveAndUpdate();
        return true;
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        playerData.gold += amount;
        SaveAndUpdate();
    }
    #endregion

    #region 属性管理
    // 增加玩家生命值上限
    public void AddLife(int value)
    {
        playerData.life += value;
        UpdateUI();
        SavePlayerData();
    }
    //在别的地方调用
    //playerController.AddLife(10);

    // 减少玩家生命上限
    public void SubtractLife(int value)
    {
        playerData.life -= value;
        UpdateUI();
        SavePlayerData();
    }

    // 增加玩家体力上限
    public void AddStrength(int value)
    {
        playerData.strength += value;
        UpdateUI();
        SavePlayerData();
    }

    // 减少玩家体力上限
    public void SubtractStrength(int value)
    {
        playerData.strength -= value;
        UpdateUI();
        SavePlayerData();
    }

    // 增加玩家心情
    public void AddMood(int value)
    {
        playerData.mood = Mathf.Clamp(playerData.mood + value, 0, 100);
        UpdateUI();
        SavePlayerData();
    }

    // 减少玩家心情
    public void SubtractMood(int value)
    {
        playerData.mood = Mathf.Clamp(playerData.mood - value, 0, 100);
        UpdateUI();
        SavePlayerData();
    }

    // 增加玩家失序值
    public void AddMuddledness(int value)
    {
        playerData.muddledness = Mathf.Clamp(playerData.muddledness + value, 0, 100);
        UpdateUI();
        SavePlayerData();
    }

    // 减少玩家失序值
    public void SubtractMuddledness(int value)
    {
        playerData.muddledness = Mathf.Clamp(playerData.muddledness - value, 0, 100);
        UpdateUI();
        SavePlayerData();
    }

    // 增加声望
    public void AddFame(int value)
    {
        playerData.fame = Mathf.Max(playerData.fame + value, 0);
        UpdateUI();
        SavePlayerData();
    }

    public void SubtractFame(int value)
    {
        playerData.fame = Mathf.Max(playerData.fame - value, 0);
        UpdateUI();
        SavePlayerData();
    }

    // 道德
    public void AddMorality(int value)
    {
        playerData.morality = Mathf.Min(playerData.morality + value, 100);
        UpdateUI();
        SavePlayerData();
    }

    public void SubtractMorality(int value)
    {
        playerData.morality -= value; // 允许负数
        UpdateUI();
        SavePlayerData();
    }

    // 智力
    public void AddIntelligence(int value)
    {
        playerData.intelligence = Mathf.Clamp(playerData.intelligence + value, 0, 100);
        UpdateUI();
        SavePlayerData();
    }

    public void SubtractIntelligence(int value)
    {
        playerData.intelligence = Mathf.Clamp(playerData.intelligence - value, 0, 100);
        UpdateUI();
        SavePlayerData();
    }

    // 理解力
    public void AddComprehension(int value)
    {
        playerData.comprehension = Mathf.Clamp(playerData.comprehension - value, 0, 100);
        UpdateUI();
        SavePlayerData();
    }

    public void SubtractComprehension(int value)
    {
        playerData.comprehension = Mathf.Clamp(playerData.comprehension - value, 0, 100);
        UpdateUI();
        SavePlayerData();
    }

    // 天赋
    public void AddTalent(int value)
    {
        playerData.talent = Mathf.Clamp(playerData.talent + value, 0, 100);
        UpdateUI();
        SavePlayerData();
    }

    public void SubtractTalent(int value)
    {
        playerData.talent = Mathf.Clamp(playerData.talent - value, 0, 100);
        UpdateUI();
        SavePlayerData();
    }

    // 社交
    public void AddSociety(int value)
    {
        playerData.society = Mathf.Clamp(playerData.society + value, 0, 100);
        UpdateUI();
        SavePlayerData();
    }

    public void SubtractSociety(int value)
    {
        playerData.society = Mathf.Clamp(playerData.society - value, 0, 100);
        UpdateUI();
        SavePlayerData();
    }

    // 增加玩家仓库容量
    public void AddWarehouse(int value)
    {
        playerData.warehouse += value;
        UpdateUI();
        SavePlayerData();
    }

    // 减少玩家仓库容量
    public void SubtractWarehouse(int value)
    {
        playerData.warehouse -= value;
        UpdateUI();
        SavePlayerData();
    }
    #endregion

    #region 更新UI
    private void UpdateGoldUI(int newGold)
    {
        GoldUI.text = "低保: ￥" + newGold;
    }

    private void UpdateUI()
    {
        PlayerNameUI.text = "姓名: " + playerData.name;
        PlayerLifeUI.text = "生命上限: " + playerData.life;
        PlayerStrengthUI.text = "体力上限: " + playerData.strength;
        PlayerMoodUI.text = "当前心情: " + playerData.mood;
        PlayerMuddlednessUI.text = "失序值: " + playerData.muddledness;
        PlayerWarehouseUI.text = "库容量: " + playerData.warehouse;
        UpdateGoldUI(CurrentGold);
    }
    #endregion
}