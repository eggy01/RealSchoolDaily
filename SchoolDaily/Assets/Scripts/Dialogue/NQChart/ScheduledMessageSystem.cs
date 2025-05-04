using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScheduledMessageSystem : MonoBehaviour
{
    [System.Serializable]
    public class ScheduledMessage
    {
        public string storyID; // 关联的剧情ID
        public DateTime triggerTime; // 触发时间
        public bool hasBeenTriggered; // 是否已触发
        public bool requiresPlayerOnline; // 是否需要玩家在线时触发
    }

    [SerializeField] private float checkInterval = 60f; // 检查间隔(秒)

    private List<ScheduledMessage> scheduledMessages = new List<ScheduledMessage>();
    private float timeSinceLastCheck = 0f;

    public static ScheduledMessageSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ScheduleMessage("Semester_sx_0,", new DateTime(1, 9, 23, 19, 0, 0), false);
        ScheduleMessage("Semester_sx_00,", new DateTime(1, 9, 23, 19, 0, 0), false);
        ScheduleMessage("Semester_sx_04,", new DateTime(1, 9, 30, 20, 0, 0), false);
    }

    private void Update()
    {
        // 定时消息检查
        timeSinceLastCheck += Time.deltaTime;
        if (timeSinceLastCheck >= checkInterval)
        {
            timeSinceLastCheck = 0f;
            CheckScheduledMessages();
        }
    }

    /// <summary>
    /// 安排一个新消息
    /// </summary>
    public void ScheduleMessage(string storyID, DateTime triggerTime, bool requiresPlayerOnline = true)
    {
        // 检查是否已经存在相同的未触发消息
        if (scheduledMessages.Any(m => m.storyID == storyID && !m.hasBeenTriggered))
        {
            Debug.Log($"已经存在未触发的相同剧情消息: {storyID}");
            return;
        }

        var newScheduledMsg = new ScheduledMessage
        {
            storyID = storyID,
            triggerTime = triggerTime,
            hasBeenTriggered = false,
            requiresPlayerOnline = requiresPlayerOnline
        };

        scheduledMessages.Add(newScheduledMsg);
        Debug.Log($"已安排消息: {storyID}, 触发时间: {triggerTime}");
    }

    /// <summary>
    /// 检查并触发到期的消息
    /// </summary>
    private void CheckScheduledMessages()
    {
        DateTime now = DateTime.Now;
        foreach (var msg in scheduledMessages.Where(m => !m.hasBeenTriggered && m.triggerTime <= now))
        {
            if (!msg.requiresPlayerOnline || IsPlayerOnline())
            {
                TriggerScheduledMessage(msg);
            }
        }
    }

    /// <summary>
    /// 触发定时消息
    /// </summary>
    private void TriggerScheduledMessage(ScheduledMessage msg)
    {
        msg.hasBeenTriggered = true;

        if (ChatSystem.Instance != null)
        {
            // 通过ChatSystem接收延迟剧情
            ChatSystem.Instance.ReceiveDeferredStory(msg.storyID);
            Debug.Log($"已触发定时消息: {msg.storyID}");
        }
        else
        {
            Debug.LogError("ChatSystem实例未找到，无法触发定时消息");
        }
    }

    /// <summary>
    /// 检查玩家是否在线
    /// </summary>
    private bool IsPlayerOnline()
    {
        return PlayerController.Instance != null && PlayerController.Instance.gameObject.activeInHierarchy;
    }

    /// <summary>
    /// 检查是否有指定剧情的未触发消息
    /// </summary>
    public bool HasScheduledMessage(string storyID)
    {
        return scheduledMessages.Any(m => m.storyID == storyID && !m.hasBeenTriggered);
    }

    /// <summary>
    /// 取消指定剧情的定时消息
    /// </summary>
    public void CancelScheduledMessage(string storyID)
    {
        int removed = scheduledMessages.RemoveAll(m => m.storyID == storyID && !m.hasBeenTriggered);
        Debug.Log($"已取消{removed}条{storyID}的定时消息");
    }

    #region 存档相关方法

    [System.Serializable]
    private class SaveData
    {
        public List<ScheduledMessage> scheduledMessages;
    }

    /// <summary>
    /// 获取存档数据
    /// </summary>
    public object GetSaveData()
    {
        return new SaveData
        {
            scheduledMessages = this.scheduledMessages
        };
    }

    /// <summary>
    /// 从存档加载数据
    /// </summary>
    public void LoadSaveData(object data)
    {
        if (data is SaveData saveData)
        {
            this.scheduledMessages = saveData.scheduledMessages ?? new List<ScheduledMessage>();
        }
    }

    #endregion
}
