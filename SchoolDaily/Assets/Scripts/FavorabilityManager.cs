using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FavorabilityManager : MonoBehaviour
{// 单例实例
    public static FavorabilityManager Instance { get; private set; }

    // 存储所有角色好感度
    private Dictionary<string, int> _favorData = new Dictionary<string, int>()
    {
        // 初始化默认值
        {"林风", 40},
        {"弗洛", 30}
    };

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 获取指定角色好感度
    /// </summary>
    public int Get(string npcName)
    {
        return _favorData.TryGetValue(npcName, out int value) ? value : 0;
    }

    /// <summary>
    /// 增加/减少好感度
    /// </summary>
    public void Add(string npcName, int amount)
    {
        if (!_favorData.ContainsKey(npcName))
        {
            _favorData.Add(npcName, 0);
        }

        _favorData[npcName] = Mathf.Max(0, _favorData[npcName] + amount);
        Debug.Log($"[好感度] {npcName} {(amount >= 0 ? "+" : "")}{amount} = {_favorData[npcName]}");
    }

    /// <summary>
    /// 设置特定值（用于测试或特殊事件）
    /// </summary>
    public void Set(string npcName, int value)
    {
        _favorData[npcName] = Mathf.Max(0, value);
        Debug.Log($"[好感度] {npcName} 设置为 {value}");
    }

    // 保存/加载方法（可选）
    public void SaveData()
    {
        PlayerPrefs.SetString("FavorData", JsonUtility.ToJson(_favorData));
    }

    public void LoadData()
    {
        if (PlayerPrefs.HasKey("FavorData"))
        {
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString("FavorData"), _favorData);
        }
    }
}
