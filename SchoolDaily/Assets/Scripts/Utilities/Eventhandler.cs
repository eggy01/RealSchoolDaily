using System;
using System.Collections;
using System.Collections.Generic;
using SchoolD.Dialogue;
using Unity.VisualScripting;
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

    // public static event System.Action NextDialogueEvent;
    // public static void TriggerNextDialogue()
    // {
    //     NextDialogueEvent?.Invoke();
    // }

    public static event Action OnNextDialogueEvent;

    public static void TriggerNextDialogue()
    {
        OnNextDialogueEvent?.Invoke();
    }
}