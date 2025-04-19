using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Task
{
    public string PID;          // 任务唯一ID
    public string location;     // 触发地点
    public string title;       // 任务标题
    public string reward;       // 奖励描述
    public string description; // 任务详情
    public string time;        // 触发时间

    [Header("状态")]
    public bool isCompleted;
    public bool isActive;
}

public class TaskSystem : MonoBehaviour
{
    public static TaskSystem Instance { get; private set; }

    [SerializeField]
    private List<Task> allTasks = new List<Task>();
    private Dictionary<string, Task> taskDict = new Dictionary<string, Task>();

    private void Awake()
    {
        Instance = this;
        Initialize();
    }

    private void Initialize()
    {
        // 示例数据 - 实际应从CSV/JSON加载
        allTasks = new List<Task>
        {
            new Task {
                PID = "0000001",
                location = "寝室",
                title = "开学日",
                description = "今天是开学第一天。",
                time = "9月3日"
            },
            new Task {
                PID = "0000002",
                location = "寝室",
                title = "开学日",
                description = "林风好感度+5。林风的课本不见了，帮她找找吧。",
                time = "9月3日"
            }
        };

        // 建立字典索引
        foreach (var task in allTasks)
        {
            taskDict[task.PID] = task;
        }
    }

    // 在TaskSystem中添加
    public void LoadTasksFromCSV(TextAsset csvFile)
    {
        string[] lines = csvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++) // 跳过表头
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] fields = ParseCSVLine(lines[i]);
            if (fields.Length < 5) continue;

            var task = new Task
            {
                PID = fields[0].Trim(),
                location = fields[1].Trim(),
                title = fields[2].Trim(),
                reward = fields[3].Trim(),
                description = fields[4].Trim(),
                time = fields.Length > 5 ? fields[5].Trim() : ""
            };

            allTasks.Add(task);
            taskDict[task.PID] = task;
        }
    }

    private string[] ParseCSVLine(string line)
    {
        List<string> fields = new List<string>();
        bool inQuotes = false;
        int startIndex = 0;

        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (line[i] == ',' && !inQuotes)
            {
                string field = line.Substring(startIndex, i - startIndex).Trim();
                field = field.Trim('"'); // 移除可能的引号
                fields.Add(field);
                startIndex = i + 1;
            }
        }

        // 添加最后一个字段
        string lastField = line.Substring(startIndex).Trim();
        lastField = lastField.Trim('"');
        fields.Add(lastField);

        return fields.ToArray();
    }

    // 在TaskSystem中添加核心方法
    public void StartTask(string pid)
    {
        if (taskDict.TryGetValue(pid, out Task task))
        {
            task.isActive = true;
            Debug.Log($"任务开始: {task.title}\n{task.description}");
        }
    }

    public void CompleteTask(string pid)
    {
        if (taskDict.TryGetValue(pid, out Task task))
        {
            task.isCompleted = true;
            task.isActive = false;
            ApplyRewards(task.description); // 处理奖励
            Debug.Log($"任务完成: {task.title}");
        }
    }

    private void ApplyRewards(string desc)
    {
        // 解析描述中的奖励（如"林风好感度+5"）
        if (desc.Contains("好感度+"))
        {
            string[] parts = desc.Split('+');
            string npcName = parts[0].Replace("好感度", "").Trim();
            int amount = int.Parse(parts[1].Split('。')[0]);

            FavorabilityManager.Instance.Add(npcName, amount);
        }
    }

    /// <summary>
    /// 获取单个任务完整信息
    /// </summary>
    public Task GetTask(string pid)
    {
        if (taskDict.TryGetValue(pid, out Task task))
        {
            return task;
        }
        Debug.LogWarning($"找不到PID为 {pid} 的任务");
        return null;
    }

    /// <summary>
    /// 获取所有任务列表
    /// </summary>
    public List<Task> GetAllTasks()
    {
        return new List<Task>(allTasks); // 返回副本避免外部修改
    }

    /// <summary>
    /// 按状态筛选任务
    /// </summary>
    public List<Task> GetTasksByStatus(bool wantCompleted, bool wantActive = false)
    {
        return allTasks.FindAll(t =>
            t.isCompleted == wantCompleted &&
            (wantActive ? t.isActive : true));
    }

    /// <summary>
    /// 按地点查询任务
    /// </summary>
    public List<Task> GetTasksByLocation(string location)
    {
        return allTasks.FindAll(t =>
            t.location.Equals(location, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 获取当前可接取任务（未完成且未激活）
    /// </summary>
    public List<Task> GetAvailableTasks()
    {
        return allTasks.FindAll(t => !t.isCompleted && !t.isActive);
    }

    /// <summary>
    /// 获取进行中任务
    /// </summary>
    public List<Task> GetActiveTasks()
    {
        return allTasks.FindAll(t => t.isActive && !t.isCompleted);
    }

}
