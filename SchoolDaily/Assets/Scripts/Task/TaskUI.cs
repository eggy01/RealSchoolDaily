using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskUI : MonoBehaviour
{
    [Header("UI组件")]
    public GameObject taskPanel; // 主任务面板
    public Transform taskListContent; // 任务列表父对象
    public TextMeshProUGUI taskTitleText; // 任务标题文本
    public TextMeshProUGUI taskDescriptionText; // 任务描述文本
    public TextMeshProUGUI taskLocationText; // 任务地点文本
    public TextMeshProUGUI taskTimeText; // 任务时间文本
    public Image taskStatusIcon; // 任务状态图标

    [Header("预设体")]
    public GameObject taskItemPrefab; // 任务列表项预设体

    [Header("图标")]
    public Sprite activeTaskIcon; // 进行中任务图标
    public Sprite completedTaskIcon; // 已完成任务图标
    public Sprite availableTaskIcon; // 可接取任务图标

    private void Start()
    {
        // 初始化隐藏任务面板
        taskPanel.SetActive(false);

        // 注册事件
        EventHandler.OnShowTaskPanel += ShowTaskPanel;
        EventHandler.OnUpdateTaskUI += UpdateTaskUI;
    }

    private void OnDestroy()
    {
        // 注销事件
        EventHandler.OnShowTaskPanel -= ShowTaskPanel;
        EventHandler.OnUpdateTaskUI -= UpdateTaskUI;
    }

    /// <summary>
    /// 显示/隐藏任务面板
    /// </summary>
    public void ShowTaskPanel(bool show)
    {
        taskPanel.SetActive(show);

        if (show)
        {
            // 刷新任务列表
            RefreshTaskList();

            // 默认显示第一个任务
            var tasks = TaskSystem.Instance.GetAllTasks();
            if (tasks.Count > 0)
            {
                ShowTaskDetails(tasks[0]);
            }
        }
    }

    /// <summary>
    /// 刷新任务列表
    /// </summary>
    public void RefreshTaskList()
    {
        // 清空现有列表
        foreach (Transform child in taskListContent)
        {
            Destroy(child.gameObject);
        }

        // 获取所有任务并按状态分组
        var activeTasks = TaskSystem.Instance.GetActiveTasks();

        var completedTasks = TaskSystem.Instance.GetTasksByStatus(true);

        var availableTasks = TaskSystem.Instance.GetAvailableTasks();

        // 添加进行中任务
        AddTasksToUIList(activeTasks, "进行中任务");

        // 添加可接取任务
        AddTasksToUIList(availableTasks, "可接取任务");

        // 添加已完成任务
        AddTasksToUIList(completedTasks, "已完成任务");
    }

    /// <summary>
    /// 添加任务到UI列表
    /// </summary>
    private void AddTasksToUIList(List<Task> tasks, string categoryName)
    {
        if (tasks.Count == 0) return;

        // 添加分类标题
        var categoryHeader = Instantiate(taskItemPrefab, taskListContent);

        categoryHeader.GetComponent<TaskListItem>().SetAsHeader(categoryName);

        // 添加任务项
        foreach (var task in tasks)
        {
            var taskItem = Instantiate(taskItemPrefab, taskListContent);
            var listItem = taskItem.GetComponent<TaskListItem>();

            // 设置图标
            Sprite statusIcon = task.isCompleted ? completedTaskIcon :
                              task.isActive ? activeTaskIcon : availableTaskIcon;

            listItem.Setup(task, statusIcon);

            // 添加点击事件
            listItem.GetComponent<Button>().onClick.AddListener(() => ShowTaskDetails(task));
        }
    }

    /// <summary>
    /// 显示任务详情
    /// </summary>
    public void ShowTaskDetails(Task task)
    {
        taskTitleText.text = task.title;//任务标题

        taskDescriptionText.text = task.description;//任务描述

        taskLocationText.text = $"地点: {task.location}";

        taskTimeText.text = $"时间: {task.time}";

        // 设置状态图标
        taskStatusIcon.sprite = task.isCompleted ? completedTaskIcon :
                              task.isActive ? activeTaskIcon : availableTaskIcon;
    }

    /// <summary>
    /// 更新任务UI
    /// </summary>
    public void UpdateTaskUI(string taskPID)
    {
        var task = TaskSystem.Instance.GetTask(taskPID);
        if (task != null)
        {
            // 如果任务面板是打开的，则刷新UI
            if (taskPanel.activeSelf)
            {
                RefreshTaskList();
                ShowTaskDetails(task);
            }

            // 显示任务更新提示
            ShowTaskUpdateNotification(task);
        }
    }

    /// <summary>
    /// 显示任务更新提示
    /// </summary>
    private void ShowTaskUpdateNotification(Task task)
    {
        // 这里可以实现任务更新时的特效或提示
        Debug.Log($"任务更新: {task.title}");
    }
}
