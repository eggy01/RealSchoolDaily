using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;
using System.Collections;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private GameData _tempData;
    public const int SaveSlotCount = 8; // 改为8个槽位
    public int currentSlot = -1; // 当前选择的存档槽位

    void Start()
    {
        // 初始化临时数据
        _tempData = new GameData();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 新游戏初始化
    public void NewGame(int slot)
    {
        currentSlot = slot;
        _tempData = new GameData();
        _tempData.playerData = new PlayerData(); // 明确初始化
        _tempData.gameTimeData = TimeManager.Instance.GetTimeDataForSave();
        SaveGame(slot);
    }

    // 保存游戏（指定槽位）
    public void SaveGame(int slot)
    {
        // 保存前获取最新时间数据
        _tempData.gameTimeData = TimeManager.Instance.GetTimeDataForSave();
        // 将内存数据转为正式存档
        BinaryFormatter formatter = new BinaryFormatter();
        string path = GetSavePath(slot);
        using (FileStream stream = new FileStream(path, FileMode.Create))
        {
            formatter.Serialize(stream, _tempData);
        }
    }

    // 加载游戏（指定槽位）
    public void LoadGame(int slot)
    {
        StartCoroutine(LoadGameRoutine(slot));
    }
    private IEnumerator LoadGameRoutine(int slot)
    {
        // 加载主场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("PersistScene");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        string path = GetSavePath(slot);
        if (File.Exists(path))
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    _tempData = formatter.Deserialize(stream) as GameData;
                }
                currentSlot = slot;

                // 确保按正确顺序初始化
                TimeManager.Instance.LoadTimeData(_tempData.gameTimeData);
                PackageLocalData.Instance.LoadData();
                NPCManager.Instance.LoadNPCData();

                PlayerInformation.Instance.CurrentData = _tempData.playerData;
                PlayerInformation.Instance.RefreshFromSaveData();

                PackageLocalData.Instance.ForceRefresh();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Load failed: {e.Message}");
            }
        }
    }

    public bool IsSlotEmpty(int slot)
    {
        return !File.Exists(GetSavePath(slot));
    }

    public int FindFirstEmptySlot()
    {
        for (int i = 0; i < SaveSlotCount; i++)
        {
            if (IsSlotEmpty(i)) return i;
        }
        return -1;
    }

    public bool AreAllSlotsFull()
    {
        return FindFirstEmptySlot() == -1;
    }

    public GameData GetTempData() => _tempData;

    // 获取存档路径
    private string GetSavePath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"save_{slot}.dat");
    }

    // 删除存档（可选功能）
    public void DeleteSave(int slot)
    {
        string path = GetSavePath(slot);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"已删除槽位 {slot} 的存档");
        }
    }
    // 设置当前存档槽位
    public void SetCurrentSlot(int slot)
    {
        currentSlot = slot;
    }

    //读取时间
    public GameTimeData GetTimeFromSave(int slot)
    {
        string path = GetSavePath(slot);
        if (File.Exists(path))
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    GameData data = formatter.Deserialize(stream) as GameData;
                    return data.gameTimeData;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"读取时间失败: {e.Message}");
                return null;
            }
        }
        return null;
    }
}