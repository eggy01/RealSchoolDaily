using UnityEngine;
using TMPro;
using System.IO;

public class PlayerInformation : MonoBehaviour
{
    public PlayerData playerData;
    public TextMeshProUGUI PlayerNameUI; //姓名
    public TextMeshProUGUI PlayerLifeUI; //生命上限
    public TextMeshProUGUI PlayerStrengthUI; //体力上限
    public TextMeshProUGUI PlayerMoodUI; //心情值
    public TextMeshProUGUI PlayerMuddlednessUI; //失序值
    public TextMeshProUGUI GoldUI; //金钱
    public TextMeshProUGUI PlayerWarehouseUI; // 库容量

    private string savePath = "PlayerData.json"; // JSON 文件路径

    void Start()
    {
        GoldManager.Instance.OnGoldUpdated.AddListener(UpdateGoldUI);
        // 加载玩家数据
        LoadPlayerData();
        // 如果没有加载到数据，则使用初始值
        if (string.IsNullOrEmpty(playerData.name))
        {
            playerData = new PlayerData();
        }
        UpdateUI();
    }

    void OnDestroy()
    {
        // 取消事件监听（防止内存泄漏）
        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.OnGoldUpdated.RemoveListener(UpdateGoldUI);
        }
    }

    // 保存玩家数据到 JSON 文件
    public void SavePlayerData()
    {
        string jsonPlayerData = JsonUtility.ToJson(playerData);
        File.WriteAllText(savePath, jsonPlayerData);
    }

    // 从 JSON 文件加载玩家数据
    public void LoadPlayerData()
    {
        if (File.Exists(savePath))
        {
            string jsonPlayerData = File.ReadAllText(savePath);
            playerData = JsonUtility.FromJson<PlayerData>(jsonPlayerData);
        }
        else
        {
            playerData = new PlayerData();
        }
    }

    // 增加玩家生命值
    public void AddLife(int value)
    {
        playerData.life += value;
        UpdateUI();
        SavePlayerData();
    }
    //在别的地方调用
    //playerController.AddLife(10);

    // 减少玩家生命值
    public void SubtractLife(int value)
    {
        playerData.life -= value;
        UpdateUI();
        SavePlayerData();
    }

    // 增加玩家体力
    public void AddStrength(int value)
    {
        playerData.strength += value;
        UpdateUI();
        SavePlayerData();
    }

    // 减少玩家体力
    public void SubtractStrength(int value)
    {
        playerData.strength -= value;
        UpdateUI();
        SavePlayerData();
    }

    // 增加玩家心情
    public void AddMood(int value)
    {
        playerData.mood += value;
        UpdateUI();
        SavePlayerData();
    }

    // 减少玩家心情
    public void SubtractMood(int value)
    {
        playerData.mood -= value;
        UpdateUI();
        SavePlayerData();
    }

    // 增加玩家失序值
    public void AddMuddledness(int value)
    {
        playerData.muddledness += value;
        UpdateUI();
        SavePlayerData();
    }

    // 减少玩家失序值
    public void SubtractMuddledness(int value)
    {
        playerData.muddledness -= value;
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

    // 更新 UI 方法
    private void UpdateGoldUI(int newGold)
    {
        GoldUI.text = "低保: ￥" + newGold;
    }
    void UpdateUI()
    {
        PlayerNameUI.text = "姓名: " + playerData.name;
        PlayerLifeUI.text = "生命上限: " + playerData.life.ToString();
        PlayerStrengthUI.text = "体力上限: " + playerData.strength.ToString();
        PlayerMoodUI.text = "当前心情: " + playerData.mood.ToString();
        PlayerMuddlednessUI.text = "失序值: " + playerData.muddledness.ToString();
        PlayerWarehouseUI.text = "库容量: " + playerData.warehouse.ToString();
    }
}