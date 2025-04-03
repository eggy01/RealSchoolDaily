using System;
using System.Collections;
using System.Collections.Generic;
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
}