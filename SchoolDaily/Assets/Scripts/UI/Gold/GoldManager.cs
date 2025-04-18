using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class GoldUpdateEvent : UnityEvent<int> { }

public class GoldManager : MonoBehaviour
{
    #region Singleton Pattern
    public static GoldManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        LoadGold();
    }
    #endregion

    #region 公共变量
    [Header("Configuration")]
    public int InitialGold = 100; //初始数量
    public bool EnableEncryption = true; //是否加密

    [Header("Events")]
    public GoldUpdateEvent OnGoldUpdated; //更新
    #endregion

    #region 私有变量
    private const string PlayerPrefsKey = "PlayerGold"; //储存金币数据
    private string _encryptionKey = "gold_encrypt_2023"; //加密密钥
    private int _currentGold; //当前金币数量
    #endregion

    #region Properties
    public int CurrentGold
    {
        //返回当前金币的值
        get => _currentGold;
        //设置当前金币的值
        private set
        {
            _currentGold = Mathf.Max(0, value);
            SaveGold();
            OnGoldUpdated?.Invoke(_currentGold);
        }
    }
    #endregion

    #region Public Methods

    //消耗金币
    public bool TrySpendGold(int amount)
    {
        if (amount <= 0 || CurrentGold < amount) return false;
        CurrentGold -= amount;
        Debug.Log(CurrentGold);
        return true;
    }

    //添加金币
    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        CurrentGold += amount;
    }
    #endregion

    #region 数据持久化
    private void LoadGold()
    {
        string savedData = PlayerPrefs.GetString(PlayerPrefsKey, null);
        if (string.IsNullOrEmpty(savedData))
        {
            CurrentGold = InitialGold;
            return;
        }

        try
        {
            string decryptedData = EnableEncryption
                ? XorDecrypt(savedData, _encryptionKey)
                : savedData;
            CurrentGold = int.Parse(decryptedData);
        }
        catch
        {
            Debug.LogWarning("Corrupted gold data. Resetting to initial value.");
            CurrentGold = InitialGold;
        }
    }

    private void SaveGold()
    {
        string dataToSave = EnableEncryption
            ? XorEncrypt(_currentGold.ToString(), _encryptionKey)
            : _currentGold.ToString();

        PlayerPrefs.SetString(PlayerPrefsKey, dataToSave);
        PlayerPrefs.Save();
    }
    #endregion

    #region 加密
    private string XorEncrypt(string input, string key)
    {
        char[] output = new char[input.Length];
        for (int i = 0; i < input.Length; i++)
        {
            output[i] = (char)(input[i] ^ key[i % key.Length]);
        }
        return new string(output);
    }

    private string XorDecrypt(string input, string key) => XorEncrypt(input, key);
    #endregion
}