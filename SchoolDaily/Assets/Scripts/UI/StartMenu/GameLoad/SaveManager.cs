using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;
using System.Collections;
using SchoolD.Task;

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
        SaveGame(slot, true);
    }

    // 获取存档槽位文件夹路径
    private string GetSaveFolderPath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"save_slot_{slot}");
    }

    // 确保存档目录存在
    private void EnsureSaveDirectoryExists(int slot)
    {
        string folderPath = GetSaveFolderPath(slot);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
    }

    // 保存游戏（指定槽位）
    // 保存游戏（包括剧情和任务）
    public void SaveGame(int slot, bool newGame = false)//第二个参数为是否是新游戏第一次保存
    {
        EnsureSaveDirectoryExists(slot);

        // 保存主存档
        _tempData.gameTimeData = TimeManager.Instance.GetTimeDataForSave();

        BinaryFormatter formatter = new BinaryFormatter();

        // 主存档路径
        string mainSavePath = Path.Combine(GetSaveFolderPath(slot), "main_save.dat");
        using (FileStream stream = new FileStream(mainSavePath, FileMode.Create))
        {
            formatter.Serialize(stream, _tempData);
        }

        // 保存剧情进度
        if (StoryProgressManager.Instance != null)
        {
            string storyPath = Path.Combine(GetSaveFolderPath(slot), "story_save.dat");
            if (newGame)
            {
                Debug.Log("新游戏加载");
                StoryProgressManager.Instance.LoadStoryProgressFromCSV();//新游戏时，先从文件加载基础数据}
                EventHandler.callLoadCSVCompleted();
            }
            StoryProgressManager.Instance.SaveProgress(storyPath);
        }
        // 保存任务进度
        if (TaskSystem.Instance != null)
        {
            string taskPath = Path.Combine(GetSaveFolderPath(slot), "task_save.dat");
            if (newGame)
                TaskSystem.Instance.LoadTasksFromCSV();//新游戏时，先从文件加载基础数据
            TaskSystem.Instance.SaveTasks(taskPath);
        }
        // 保存聊天进度
        if (ChatSystem.Instance != null)
        {
            string chatPath = Path.Combine(GetSaveFolderPath(slot), "chat_save.dat");
            ChatSystem.Instance.SaveChatData(chatPath);
        }

    }

    // 加载游戏（指定槽位）
    public void LoadGame(int slot)
    {
        StartCoroutine(LoadGameRoutine(slot));
    }
    private IEnumerator LoadGameRoutine(int slot)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("PersistScene");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        string folderPath = GetSaveFolderPath(slot);
        string mainSavePath = Path.Combine(folderPath, "main_save.dat");

        if (File.Exists(mainSavePath))
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream stream = new FileStream(mainSavePath, FileMode.Open))
                {
                    _tempData = formatter.Deserialize(stream) as GameData;
                }
                currentSlot = slot;

                // 加载主数据
                TimeManager.Instance.LoadTimeData(_tempData.gameTimeData);
                PackageLocalData.Instance.LoadData();
                NPCManager.Instance.LoadNPCData();
                PlayerInformation.Instance.CurrentData = _tempData.playerData;
                PlayerInformation.Instance.RefreshFromSaveData();
                PackageLocalData.Instance.ForceRefresh();

                // 加载剧情进度
                string storyPath = Path.Combine(folderPath, "story_save.dat");
                if (File.Exists(storyPath) && StoryProgressManager.Instance != null)
                {
                    StoryProgressManager.Instance.LoadStoryProgressFromCSV();//加载失败后，从文件中重新读取
                }

                // 加载任务进度
                string taskPath = Path.Combine(folderPath, "task_save.dat");
                if (File.Exists(taskPath) && TaskSystem.Instance != null)
                {
                    TaskSystem.Instance.LoadTasks(taskPath);
                }

                // 加载聊天进度
                string chatPath = Path.Combine(folderPath, "chat_save.dat");
                if (File.Exists(chatPath) && TaskSystem.Instance != null)
                {
                    ChatSystem.Instance.LoadChatData(chatPath);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Load failed: {e.Message}");
            }
        }
    }

    // 检查存档槽位是否为空
    public bool IsSlotEmpty(int slot)
    {
        string folderPath = GetSaveFolderPath(slot);
        if (!Directory.Exists(folderPath)) return true;
        return false;
        // string mainSavePath = Path.Combine(folderPath, "main_save.dat");
        // return !File.Exists(mainSavePath);
    }

    // 删除整个存档槽位
    public void DeleteSave(int slot)
    {
        string folderPath = GetSaveFolderPath(slot);
        if (Directory.Exists(folderPath))
        {
            Directory.Delete(folderPath, true);
            Debug.Log($"已删除槽位 {slot} 的所有存档");
        }
    }

    public int FindFirstEmptySlot()
    {
        for (int i = 0; i < SaveSlotCount; i++)
        {
            if (IsSlotEmpty(i)) return i;
        }
        Debug.LogWarning("没有空槽");
        return -1;
    }

    public bool AreAllSlotsFull()
    {
        return FindFirstEmptySlot() == -1;
    }

    public GameData GetTempData() => _tempData;

    // // 获取存档路径
    // private string GetSavePath(int slot)
    // {
    //     return Path.Combine(Application.persistentDataPath, $"save_{slot}.dat");
    // }

    // 设置当前存档槽位
    public void SetCurrentSlot(int slot)
    {
        currentSlot = slot;
    }

    //读取时间
    public GameTimeData GetTimeFromSave(int slot)
    {
        //string path = GetSavePath(slot);
        string path = GetSaveFolderPath(slot) + "/main_save.dat";
        Debug.LogWarning("path" + path);
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