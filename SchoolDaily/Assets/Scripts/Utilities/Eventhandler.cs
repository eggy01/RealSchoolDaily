using System;
using SchoolD.Dialogue;
using UnityEngine;

public class EventHandler
{

    public static event Action<int, int> GameMinuteEvent;

    public static void CallGameMinuteEvent(int minute, int hour)
    {
        GameMinuteEvent?.Invoke(minute, hour);
    }

    public static event Action<int, int, int, int, Season, int, int> GameDateEvent;

    public static void CallGameDateEvent(int hour, int day, int month, int year,
                                       Season season, int weekDay, int term)
    {
        GameDateEvent?.Invoke(hour, day, month, year, season, weekDay, term);
    }

    //天气相关
    // 添加日期变化事件
    public static event System.Action OnDayChangedEvent;
    public static void CallOnDayChangedEvent()
    {
        OnDayChangedEvent?.Invoke();
    }

    //昼夜相关
    //十分钟更新一次光照
    public static event System.Action TenMinuteChanged;
    public static void CallTenMinuteChanged()
    {
        TenMinuteChanged?.Invoke();
    }
    //一小时更新一次光照
    public static event System.Action OnHourChangedEvent;
    public static void CallOnHourChangedEvent()
    {
        OnHourChangedEvent?.Invoke();
    }


    //场景转换相关
    public static event Action<String, Vector3> TransitionEvent;
    public static void CallTransitionEvent(String sceneName, Vector3 pos)
    {
        TransitionEvent?.Invoke(sceneName, pos);
    }

    //场景卸载之前
    public static event Action BeforeScenUnLoadEvent;
    public static void CallBeforeSceneUnLoadEvent()
    {
        BeforeScenUnLoadEvent?.Invoke();
    }

    //场景加载之后
    public static event Action AfterScenLoadEvent;
    public static void CallAfterScenLoadEvent()
    {
        Debug.Log("CallAfterSceneLoadEvent is being called");
        AfterScenLoadEvent?.Invoke();
    }


    //移动坐标
    public static event Action<Vector3> MoveToPositionEvent;

    public static void CallMoveToPositionEvent(Vector3 targetPosition)
    {
        MoveToPositionEvent?.Invoke(targetPosition);
    }

    //显示对话
    public static event Action<DialoguePiece> ShowDialogueEvent;
    public static void CallShowDialogueEvent(DialoguePiece piece)
    {
        ShowDialogueEvent?.Invoke(piece);
    }

    public static event System.Action NextDialogueEvent;
    public static void TriggerNextDialogue()
    {
        NextDialogueEvent?.Invoke();
    }


    //开启新对话
    public static event Action<string, System.Action> OnStartNewDialogueEvent;
    public static void CallStartNewDialogueEvent(string newDialogueFileName, System.Action onDialogueComplete = null)
        => OnStartNewDialogueEvent?.Invoke(newDialogueFileName, onDialogueComplete);



    // 任务相关事件
    public static event Action OnShowTaskPanel;
    public static event Action<string> OnUpdateTaskUI;
    public static void CallShowTaskPanel()
    {
        OnShowTaskPanel?.Invoke();
    }

    public static void CallUpdateTaskUI(string taskPID)
    {
        OnUpdateTaskUI?.Invoke(taskPID);
    }

    //有新任务
    public static event Action<bool> OnUnlockNewTask;
    public static void callOnUnlockNewTask(bool isNewNOtComplete)
    {
        OnUnlockNewTask?.Invoke(isNewNOtComplete);
    }

    // 跳转剧情
    public static event Action<string, string> OnLoadDialogueByIndex;
    public static void CallLoadDialogueByIndex(string index, string dialogueID)
    {
        OnLoadDialogueByIndex?.Invoke(index, dialogueID);
    }

    //聚焦相机//画面展示
    public static event Action OnFocusCamear;
    public static void HaveOnFocusCamear()
    {
        OnFocusCamear?.Invoke();
    }

    //获得物品
    public static event Action<ItemData> OnItemAdded;
    public static void CallItemAdded(ItemData item)
    {
        OnItemAdded?.Invoke(item);
    }

    //天气变化和剧情筛选
    public static event Action<string> OnDateChanged; // 新增日期变化事件
    public static void CallDateChangedEvent(string currentDate)
    {
        OnDateChanged?.Invoke(currentDate);
    }

    //新
    /// <summary>
    /// 对话结束事件
    /// 参数：string - 结束的对话图表名称
    /// </summary>
    public static event Action<string> OnDialogueEnd;

    /// <summary>
    /// 触发对话结束事件
    /// </summary>
    /// <param name="graphName">结束的对话图表名称</param>
    public static void CallOnDialogueEnd(string graphName)
    {
        OnDialogueEnd?.Invoke(graphName);
    }

    // 其他可能用到的事件（示例）
    public static event Action<string> OnDialogueStart; // 对话开始事件
    public static event Action<string, int> OnChoiceSelected; // 选项选择事件

}