using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace SchoolD.Task
{
    [Serializable]
    public enum TaskState
    {
        //子任务，未开始，正在运行，已完成，已停止
        Active,     // 正在运行 (for child tasks)
        Completed,  // 已完成 (for child tasks)

        //父任务，未开始，正在运行,已挂起，,已完成，已停止
        NoStarted,      //未开始
        Suspended,  // 已挂起 
        Stopped     // 已停止 
    }

    [Serializable]
    public class Task
    {
        public string PID;          // 任务唯一ID
        public string parentPID;    // 父任务ID (为空表示是父任务)
        public string location;     // 触发地点
        public string title;       // 任务标题
        public string reward;       // 奖励描述
        public string description; // 任务详情
        public string time;        // 截止时间

        [Header("状态")]
        public TaskState state;

        // 是否是父任务
        public bool IsParentTask => string.IsNullOrEmpty(parentPID);

    }

    public class TaskSystem : MonoBehaviour
    {
        public static TaskSystem Instance { get; private set; }

        [SerializeField]
        private List<Task> allTasks = new List<Task>();
        private Dictionary<string, Task> taskDict = new Dictionary<string, Task>();//存所有的任务，pid:task
        private Dictionary<string, List<Task>> parentChildMap = new Dictionary<string, List<Task>>();//parent PID:task
        public TextAsset taskCsvFile;

        [System.Serializable]//存档
        private class TaskSaveData
        {
            public List<Task> savedTasks = new List<Task>();
        }

        private void Awake()
        {
            Instance = this;
            Initialize();
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))//
            {
                Debug.Log("按下T");
                EventHandler.CallShowTaskPanel(); // 显示或显示面板
            }
        }

        private void Initialize()
        {
            LoadTasks();

        }

        private void RebuildDictionaries()
        {
            taskDict.Clear();
            parentChildMap.Clear();

            foreach (var task in allTasks)
            {
                taskDict[task.PID] = task;

                if (!task.IsParentTask)//子任务
                {
                    if (!parentChildMap.ContainsKey(task.parentPID))//没有其父任务信息
                    {
                        parentChildMap[task.parentPID] = new List<Task>();//创建一个父任务列表
                    }
                    parentChildMap[task.parentPID].Add(task);
                }
            }
        }

        public void LoadTasksFromCSV(TextAsset csvFile)
        {
            string[] lines = csvFile.text.Split('\n');

            for (int i = 1; i < lines.Length; i++) // 跳过表头
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                string[] fields = ParseCSVLine(lines[i]);
                if (fields.Length < 6) continue;

                // 清理和规范化所有字段
                for (int j = 0; j < fields.Length; j++)
                {
                    fields[j] = fields[j]?.Trim() ?? "";
                }

                var task = new Task
                {
                    PID = fields[0],
                    parentPID = fields[1],
                    location = fields[2],
                    title = fields[3],
                    reward = fields[4],
                    description = fields[5],
                    time = fields.Length > 6 ? fields[6].Replace(";", ":") : "", // 统一时间分隔符
                    state = TaskState.NoStarted // 默认状态
                };

                // 只有明确提供了状态时才解析
                if (fields.Length > 7 && !string.IsNullOrEmpty(fields[7]))
                {
                    if (Enum.TryParse(fields[7], out TaskState parsedState))
                    {
                        task.state = parsedState;
                    }
                }

                allTasks.Add(task);
            }

            RebuildDictionaries();
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
                    field = field.Trim('"');
                    fields.Add(field);
                    startIndex = i + 1;
                }
            }

            string lastField = line.Substring(startIndex).Trim();
            lastField = lastField.Trim('"');
            fields.Add(lastField);

            return fields.ToArray();
        }

        public void StartTask(string pid)
        {
            if (taskDict.TryGetValue(pid, out Task task) && !task.IsParentTask)
            {
                // 只能从未开始状态开始任务
                if (task.state == TaskState.NoStarted)
                {
                    task.state = TaskState.Active;
                    Debug.Log($"任务开始: {task.title}\n{task.description}");

                    TipController.Instance.ShowTaskTip(true);
                    //EventHandler.callOnUnlockNewTask(true);

                    // 更新父任务状态为已挂起
                    UpdateParentTaskState(task.parentPID, true);
                    SaveTasks(); // 新增
                }
                else
                {
                    Debug.LogWarning($"任务 {pid} 当前状态为 {task.state}，不能从该状态开始");
                }
            }
        }

        public void CompleteTask(string pid)
        {
            if (taskDict.TryGetValue(pid, out Task task) && !task.IsParentTask)
            {
                // 只能从进行中状态完成任务
                if (task.state == TaskState.Active)
                {
                    task.state = TaskState.Completed;
                    ApplyRewards(task.reward);
                    Debug.Log($"任务完成: {task.title}");
                    EventHandler.callOnUnlockNewTask(false);

                    // 检查父任务状态是否需要更新
                    UpdateParentTaskState(task.parentPID, false);

                    TipController.Instance.ShowTaskTip(false);
                    //EventHandler.CallUpdateTaskUI(pid);
                    SaveTasks(); // 新增
                }
                else
                {
                    Debug.LogWarning($"任务 {pid} 当前状态为 {task.state}，不能从该状态完成");
                }
            }
        }

        private void UpdateParentTaskState(string parentPID, bool childStarted)
        {
            if (string.IsNullOrEmpty(parentPID)) return;

            if (parentChildMap.TryGetValue(parentPID, out List<Task> children) &&
                taskDict.TryGetValue(parentPID, out Task parentTask))
            {
                bool allCompleted = true;
                bool anyActive = false;
                bool anyNoStarted = false;

                // 检查所有子任务状态
                foreach (var child in children)
                {
                    if (child.state != TaskState.Completed)
                    {
                        allCompleted = false;
                    }

                    if (child.state == TaskState.Active)
                    {
                        anyActive = true;
                    }

                    if (child.state == TaskState.NoStarted)
                    {
                        anyNoStarted = true;
                    }
                }

                // 状态判断优先级：
                // 1. 所有子任务完成 -> 父任务完成
                // 2. 有子任务进行中 -> 父任务进行中
                // 3. 其他情况（有未开始但无进行中） -> 父任务挂起
                if (allCompleted)
                {
                    parentTask.state = TaskState.Completed;
                    ApplyRewards(parentTask.reward);
                }
                else if (anyActive)
                {
                    parentTask.state = TaskState.Active; // 有子任务运行则父任务设为进行中
                }
                else
                {
                    parentTask.state = TaskState.Suspended; // 无子任务运行则挂起
                }

                // 保留childStarted的原始逻辑（如有新子任务开始需要特殊处理）
                if (childStarted && parentTask.state != TaskState.Active)
                {
                    parentTask.state = TaskState.Suspended;
                }

            }
        }

        private void ApplyRewards(string desc)
        {
            RewardManager.Instance.ApplyRewards(desc);
        }

        public Task GetTask(string pid)
        {
            if (taskDict.TryGetValue(pid, out Task task))
            {
                return task;
            }
            Debug.LogWarning($"找不到PID为 {pid} 的任务");
            return null;
        }

        public List<Task> GetAllTasks()
        {
            return new List<Task>(allTasks);
        }

        public List<Task> GetTasksByStatus(TaskState state)
        {
            return allTasks.FindAll(t => t.state == state);
        }

        public List<Task> GetTasksByLocation(string location)
        {
            return allTasks.FindAll(t =>
                t.location.Equals(location, StringComparison.OrdinalIgnoreCase) &&
                !t.IsParentTask); // 通常只显示子任务在位置中
        }

        public List<Task> GetAvailableTasks()
        {
            return allTasks.FindAll(t =>
                !t.IsParentTask &&
                t.state == TaskState.Active);
        }

        public List<Task> GetActiveTasks()
        {
            return allTasks.FindAll(t =>
                !t.IsParentTask &&
                t.state == TaskState.Active);
        }

        public List<Task> GetChildTasks(string parentPID)
        {
            if (parentChildMap.TryGetValue(parentPID, out List<Task> children))
            {
                return new List<Task>(children);
            }
            return new List<Task>();
        }

        public List<Task> GetParentTasks()
        {
            return allTasks.FindAll(t => t.IsParentTask);
        }

        /// <summary>
        /// 检查并标记过期的子任务
        /// </summary>
        private void CheckExpiredChildTasks(DateTime currentTime)
        {
            foreach (var task in allTasks)
            {
                // 只处理子任务且状态是未开始/进行中
                if (!task.IsParentTask &&
                   (task.state == TaskState.NoStarted || task.state == TaskState.Active))
                {
                    if (TryParseTaskTime(task.time, out DateTime deadline) &&
                        currentTime > deadline)
                    {
                        task.state = TaskState.Stopped;
                        Debug.Log($"子任务已过期: {task.title} (PID: {task.PID})");

                        // 触发事件（可选）
                        //EventHandler.OnTaskExpired?.Invoke(task.PID);
                    }
                }
            }
        }

        /// <summary>
        /// 检查并处理过期的父任务
        /// </summary>
        private void CheckExpiredParentTasks(DateTime currentTime)
        {
            foreach (var task in allTasks)
            {
                // 只处理父任务且状态不是已停止/已完成
                if (task.IsParentTask &&
                    task.state != TaskState.Stopped &&
                    task.state != TaskState.Completed)
                {
                    bool shouldStopParent = false;

                    // 情况1：父任务自身时间过期
                    if (TryParseTaskTime(task.time, out DateTime parentDeadline) &&
                        currentTime > parentDeadline)
                    {
                        shouldStopParent = true;
                    }
                    // 情况2：所有子任务已过期/完成
                    else if (AreAllChildTasksFinalized(task.PID))
                    {
                        shouldStopParent = true;
                    }

                    if (shouldStopParent)
                    {
                        task.state = TaskState.Stopped;
                        Debug.Log($"父任务已停止: {task.title} (PID: {task.PID})");
                    }
                }
            }
        }

        /// <summary>
        /// 检查所有子任务是否都已结束（完成或停止）
        /// </summary>
        private bool AreAllChildTasksFinalized(string parentPID)
        {
            if (parentChildMap.TryGetValue(parentPID, out var children))
            {
                foreach (var child in children)
                {
                    if (child.state != TaskState.Completed &&
                        child.state != TaskState.Stopped)
                    {
                        return false;
                    }
                }
                return true; // 所有子任务已结束
            }
            return false; // 没有子任务视为未结束
        }

        /// <summary>
        /// 支持多种时间格式的解析
        /// </summary>
        private bool TryParseTaskTime(string timeStr, out DateTime result)
        {
            result = DateTime.MinValue;
            if (string.IsNullOrEmpty(timeStr)) return false;

            try
            {
                // 统一处理中文日期符号
                timeStr = timeStr.Replace("年", "/")
                                .Replace("月", "/")
                                .Replace("日", "")
                                .Replace("时", ":")
                                .Replace("分", "");

                // // 自动补全年份（如果不存在）
                // if (!timeStr.Contains("/") && TimeManager.Instance != null)
                // {
                //     timeStr = $"{TimeManager.Instance.CurrentYear}/{timeStr}";
                // }

                return DateTime.TryParse(timeStr, out result);
            }
            catch
            {
                Debug.LogWarning($"时间解析失败: {timeStr}");
                return false;
            }
        }

        //存档
        private string savePath => Path.Combine(Application.persistentDataPath, "tasksave.dat");

        public void SaveTasks()
        {
            TaskSaveData data = new TaskSaveData();
            data.savedTasks = new List<Task>(allTasks);

            string json = JsonUtility.ToJson(data);
            File.WriteAllText(savePath, json);
        }

        public void LoadTasks()
        {
            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                TaskSaveData data = JsonUtility.FromJson<TaskSaveData>(json);
                allTasks = new List<Task>(data.savedTasks);
                RebuildDictionaries();
            }
            else
            {
                LoadTasksFromCSV(taskCsvFile); // 如果没有存档，从CSV初始化
            }
        }

        private void OnApplicationQuit()
        {
            SaveTasks();
        }
    }
}