using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SchoolD.Task
{
    public class TaskUI : MonoBehaviour
    {
        [Header("UI组件")]
        public GameObject taskPanel; // 主任务面板
        public GameObject taskDetailPanel; // 任务详情面板
        public TextMeshProUGUI taskTitleText; // 任务标题文本
        public Transform taskListContent; // 任务列表父对象

        public TextMeshProUGUI taskNameText; // 任务名称文本
        public TextMeshProUGUI taskPidText; // 任务编号文本
        public TextMeshProUGUI taskDescriptionText; // 任务描述文本
        public TextMeshProUGUI taskLocationText; // 任务地点文本
        public TextMeshProUGUI taskTimeText; // 任务时间文本
        public TextMeshProUGUI taskStatusText; // 任务时间文本

        public Animator LeftoptionMove;//任务控制器
        public GameObject TaskTip;


        [Header("预设体")]
        public GameObject taskItemPrefab; // 任务列表项预设体

        // 新增的翻页组件
        [Header("翻页控制")]
        public Button prevButton;
        public Button nextButton;
        public TextMeshProUGUI pageText;

        private List<Task> allParentTasks = new List<Task>();
        private int currentPageIndex = 0;

        private void Start()
        {
            taskPanel.SetActive(false);

            // 初始化翻页按钮
            prevButton.onClick.AddListener(ShowPreviousParentTask);
            nextButton.onClick.AddListener(ShowNextParentTask);

            EventHandler.OnShowTaskPanel += ShowTaskPanel;
            EventHandler.OnUpdateTaskUI += UpdateTaskUI;

        }

        private void OnDestroy()
        {
            EventHandler.OnShowTaskPanel -= ShowTaskPanel;
            EventHandler.OnUpdateTaskUI -= UpdateTaskUI;
        }

        public void ShowTaskPanel()//关闭或显示任务面板
        {
            taskPanel.SetActive(!taskPanel.activeSelf);

            if (taskPanel.activeSelf)
            {
                // 获取已挂起父任务
                allParentTasks = TaskSystem.Instance.GetParentTasks()
                    .FindAll(t => t.state == TaskState.Active);
                currentPageIndex = 0;

                if (allParentTasks.Count > 0)
                {
                    ShowCurrentParentTask();
                }
            }
        }

        private void ShowCurrentParentTask()
        {
            // 显示当前父任务信息
            var parentTask = allParentTasks[currentPageIndex];

            // 使用原有UI组件显示信息
            taskTitleText.text = parentTask.title;

            // 刷新子任务列表
            RefreshChildTasks(parentTask.PID);

            // 更新翻页状态
            UpdatePageControls();
        }

        private void RefreshChildTasks(string parentPID)
        {
            // 清空现有列表（保持原有方式）
            foreach (Transform child in taskListContent)
            {
                Destroy(child.gameObject);
            }

            // 只显示进行中的子任务
            var activeChildTasks = TaskSystem.Instance.GetChildTasks(parentPID)
                .FindAll(t => t.state == TaskState.Active);

            foreach (var task in activeChildTasks)
            {
                var taskItem = Instantiate(taskItemPrefab, taskListContent);
                var listItem = taskItem.GetComponent<TaskListItem>();

                // 设置任务项内容
                listItem.Setup(task);

                // 为任务项添加点击监听
                var button = taskItem.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => showTaskDetail(task));
                }
                else
                {
                    Debug.LogWarning("任务项预制体上没有Button组件，无法添加点击事件");
                }
            }
        }
        public void showTaskDetail(Task task)
        {
            taskDetailPanel.gameObject.SetActive(true);
            taskNameText.text = task.title;
            taskPidText.text = task.PID;
            taskStatusText.text = GetStateDisplayName(task.state);
            taskDescriptionText.text = task.description;
            taskTimeText.text = task.time;
            taskLocationText.text = task.location;
        }

        private void ShowPreviousParentTask()
        {
            if (currentPageIndex > 0)
            {
                currentPageIndex--;
                ShowCurrentParentTask();
            }
        }

        private void ShowNextParentTask()
        {
            if (currentPageIndex < allParentTasks.Count - 1)
            {
                currentPageIndex++;
                ShowCurrentParentTask();
            }
        }

        private void UpdatePageControls()
        {
            // 更新页码显示
            pageText.text = $"{currentPageIndex + 1}/{allParentTasks.Count}";

            // 更新按钮状态
            prevButton.interactable = currentPageIndex > 0;
            nextButton.interactable = currentPageIndex < allParentTasks.Count - 1;
        }

        public void UpdateTaskUI(string taskPID)
        {
            if (taskPanel.activeSelf)
            {
                // 刷新当前显示
                ShowCurrentParentTask();
            }
        }
        //private bool isShowingTip = false;
        // public IEnumerator ShowNewTaskTip(bool isNewNOtComplete)//任务的提示
        // {
        //     if (isShowingTip) yield break;
        //     if (isNewNOtComplete)
        //         TaskTip.GetComponentInChildren<TextMeshProUGUI>().text = "解锁新任务！";
        //     else
        //         TaskTip.GetComponentInChildren<TextMeshProUGUI>().text = "任务完成！";
        //     LeftoptionMove.SetBool("haveNewTask", true);
        //     yield return new WaitForSeconds(1f);
        //     LeftoptionMove.SetBool("haveNewTask", false);

        //     isShowingTip = false;
        // }


        // 添加一个转换方法
        private string GetStateDisplayName(TaskState state)
        {
            switch (state)
            {
                case TaskState.NoStarted: return "未开始";
                case TaskState.Active: return "进行中";
                case TaskState.Completed: return "已完成";
                case TaskState.Suspended: return "已挂起";
                case TaskState.Stopped: return "已停止";
                default: return "未知状态";
            }
        }
    }

}
