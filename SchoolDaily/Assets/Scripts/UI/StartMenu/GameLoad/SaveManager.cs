using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    // 当前内存中的临时数据（尚未保存）
    private GameData _tempData;

    // 存档槽位数量
    public const int SaveSlotCount = 3;
    // 当前选择的存档槽位
    public int currentSlot = 0;

    void Start()
    {
        // 游戏启动时加载最后一次保存的正式数据
        LoadGame(currentSlot);
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
        string path = GetSavePath(slot);
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                _tempData = formatter.Deserialize(stream) as GameData;
            }

            // 加载时间数据
            TimeManager.Instance.LoadTimeData(_tempData.gameTimeData);
            PackageLocalData.Instance.LoadData();
            NPCManager.Instance.LoadNPCData();
            PlayerInformation.Instance.CurrentData = _tempData.playerData;
            PlayerInformation.Instance.RefreshFromSaveData();

            PackageLocalData.Instance.ForceRefresh();
            Debug.Log($"成功加载槽位 {slot} 的存档");
        }
        else
        {
            // 新游戏初始化时间
            TimeManager.Instance.NewGameTime();
        }
    }

    public GameData GetTempData()
    {
        return _tempData;
    }

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
}