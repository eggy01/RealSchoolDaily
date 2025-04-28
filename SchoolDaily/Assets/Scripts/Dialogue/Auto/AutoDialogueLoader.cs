using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SchoolD.Dialogue;

public class AutoDialogueLoader : MonoBehaviour
{
    public TextAsset autoDialogueConfig;
    public String currentDate;

    [Header("调试设置")]
    public bool showFilterLogs = true;
    public float dailyCheckInterval = 60f; // 每天检查间隔（秒）

    private Dictionary<string, GameObject> loadedTriggers = new Dictionary<string, GameObject>();

    void Start()
    {
        currentDate = TimeManager.Instance.GetCurrentDateTime();
        ClearAllTriggers();
        LoadTriggersForCurrentDay(currentDate);
    }
    void OnEnable()
    {
        EventHandler.OnDateChanged += OnDateChangedHandler;
    }

    void OnDisable()
    {
        EventHandler.OnDateChanged -= OnDateChangedHandler;
    }

    private void OnDateChangedHandler(string newDate)
    {
        currentDate = newDate;
        ClearTimeLimitedTriggers();
        LoadTriggersForCurrentDay(currentDate);
    }

    private void LoadTriggersForCurrentDay(string currentDate)
    {

        var lines = SimpleCSVParser.Parse(autoDialogueConfig);
        int totalCount = 0, loadedCount = 0;

        foreach (var line in lines)
        {
            totalCount++;
            Debug.Log(totalCount + line["TimeLimit"]);
            if (ShouldLoadTrigger(line))
            {
                if (CreateTrigger(line))
                {
                    loadedCount++;
                    RegisterDialogue(line);
                }
            }
            else if (showFilterLogs)
            {
                Debug.Log($"跳过剧情 [{line["ID"]}]，时间限制 {line["TimeLimit"]} 不符合当前日期: {currentDate:MM-dd}");
            }
        }

        Debug.Log($"当日剧情加载完成: 共{totalCount}条，加载{loadedCount}条，跳过{totalCount - loadedCount}条");
    }

    // 只清理有时间限制的触发器
    private void ClearTimeLimitedTriggers()
    {
        List<string> toRemove = new List<string>();

        foreach (var kvp in loadedTriggers)
        {
            var line = SimpleCSVParser.Parse(autoDialogueConfig)
                       .FirstOrDefault(l => l["ID"] == kvp.Key);

            // 如果找到配置且有时间限制
            if (line != null && !string.IsNullOrEmpty(line["TimeLimit"]))
            {
                Destroy(kvp.Value);
                toRemove.Add(kvp.Key);

                if (showFilterLogs)
                    Debug.Log($"清理时间限制触发器: {kvp.Key}");
            }
        }

        // 从字典中移除
        foreach (var key in toRemove)
        {
            loadedTriggers.Remove(key);
        }
    }

    private void ClearAllTriggers()
    {
        foreach (var triggerObj in loadedTriggers.Values)
        {
            Destroy(triggerObj);
        }
        loadedTriggers.Clear();
    }

    private bool ShouldLoadTrigger(Dictionary<string, string> line)
    {
        Debug.Log("rrrr");
        // 无条件限制的剧情始终加载
        if (string.IsNullOrEmpty(line["TimeLimit"]))
            return true;

        return line["TimeLimit"].Equals(currentDate);
    }


    private bool CreateTrigger(Dictionary<string, string> line)
    {
        try
        {
            var triggerObj = new GameObject($"AutoTrigger_{line["ID"]}");
            triggerObj.transform.SetParent(this.transform);

            AutoDialogueTrigger trigger = null;

            // 处理复合条件
            if (line["Condition"].Contains(";"))
            {
                trigger = triggerObj.AddComponent<MultiConditionTrigger>();
                ((MultiConditionTrigger)trigger).conditions = line["Condition"];
            }
            else if (line["TriggerType"] == "Item")
            {
                trigger = triggerObj.AddComponent<ItemAcquiredTrigger>();
                ((ItemAcquiredTrigger)trigger).requiredItemID = line["Condition"];
            }
            else if (line["TriggerType"] == "Time")
            {
                trigger = triggerObj.AddComponent<TimeConditionTrigger>();
                ((TimeConditionTrigger)trigger).timeCondition = line["Condition"];
            }

            // if (trigger != null)
            // {
            //     trigger.dialogueID = line["ID"];

            //     // // 添加到活跃触发器字典
            //     // if (!activeTriggers.ContainsKey(line["ID"]))
            //     //     activeTriggers[line["ID"]] = new List<AutoDialogueTrigger>();
            //     // activeTriggers[line["ID"]].Add(trigger);

            //     return true;
            // }
        }
        catch (Exception e)
        {
            Debug.LogError($"创建触发器失败 [{line["ID"]}]: {e}");
        }

        return false;
    }

    private void RegisterDialogue(Dictionary<string, string> line)
    {
        try
        {
            bool isRepeatable = line["Type"] == "Repeatable";
            var dialogueData = DialogueCSVReader.LoadCSVFromResources(line["DialogueFile"]);

            DialogueManager.Instance.RegisterAutoTrigger(
                line["ID"],
                dialogueData,
                shouldMarkComplete: !isRepeatable
            );

            if (showFilterLogs)
                Debug.Log($"已加载剧情 [{line["ID"]}]，文件: {line["DialogueFile"]}，类型: {(isRepeatable ? "可重复" : "一次性")}");
        }
        catch (Exception e)
        {
            Debug.LogError($"注册对话失败 [{line["ID"]}]: {e}");
        }
    }

    // // 提供外部访问当前活跃触发器的方法
    // public List<DialogueTriggerBase> GetActiveTriggers(string dialogueID)
    // {
    //     return activeTriggers.TryGetValue(dialogueID, out var triggers) ? triggers : null;
    // }
}