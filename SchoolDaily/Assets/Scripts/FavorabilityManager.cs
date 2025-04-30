using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> keys = new List<TKey>();
    [SerializeField] private List<TValue> values = new List<TValue>();

    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        foreach (var pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        Clear();
        for (int i = 0; i < keys.Count; i++)
            Add(keys[i], values[i]);
    }
}

public class FavorabilityManager : MonoBehaviour
{
    public static FavorabilityManager Instance { get; private set; }

    [SerializeField]
    private SerializableDictionary<string, int> _favorData = new SerializableDictionary<string, int>()
    {
        {"林风", 30}, {"弗洛", 30}, {"学业导师",0}, {"宁芷",0}, {"纪远行",0}
    };

    private readonly object _lock = new object();
    private bool _isDirty;

    public delegate void FavorabilityChangedHandler(string npcName, int newValue);
    public static event FavorabilityChangedHandler OnFavorabilityChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadData();
    }

    private void Update()
    {
        if (_isDirty)
        {
            SaveData();
            _isDirty = false;
        }
    }

    public int Get(string npcName) =>
        _favorData.TryGetValue(npcName, out int value) ? value : 0;

    public void Add(string npcName, int amount)
    {
        if (string.IsNullOrEmpty(npcName)) return;

        lock (_lock)
        {
            if (!_favorData.ContainsKey(npcName))
                _favorData.Add(npcName, 0);

            _favorData[npcName] = Mathf.Clamp(_favorData[npcName] + amount, 0, 100);
            _isDirty = true;
            OnFavorabilityChanged?.Invoke(npcName, _favorData[npcName]);
        }
    }

    public void Set(string npcName, int value)
    {
        lock (_lock)
        {
            _favorData[npcName] = Mathf.Clamp(value, 0, 100);
            _isDirty = true;
            OnFavorabilityChanged?.Invoke(npcName, _favorData[npcName]);
        }
    }

    private void SaveData() =>
        PlayerPrefs.SetString("FavorData", JsonUtility.ToJson(_favorData));

    private void LoadData()
    {
        if (PlayerPrefs.HasKey("FavorData"))
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString("FavorData"), _favorData);
    }
}