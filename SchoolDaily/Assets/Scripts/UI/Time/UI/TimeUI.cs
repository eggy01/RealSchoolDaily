using UnityEngine;
using TMPro;

public class TimeUI : MonoBehaviour
{
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI weekDayText;
    public TextMeshProUGUI termText;

    private readonly string[] weekDayNames = { "一", "二", "三", "四", "五", "六", "日" };

    private void OnEnable()
    {
        EventHandler.GameMinuteEvent += OnGameMinuteEvent;
        EventHandler.GameDateEvent += OnGameDateEvent;
    }

    private void OnDisable()
    {
        EventHandler.GameMinuteEvent -= OnGameMinuteEvent;
        EventHandler.GameDateEvent -= OnGameDateEvent;
    }

    private void OnGameDateEvent(int hour, int day, int month, int year,
                              Season season, int weekDay, int term)
    {
        dateText.text = $"{month:00}/{day:00}";
        weekDayText.text = weekDayNames[weekDay - 1];
        termText.text = $"第{term}学期";
    }

    private void OnGameMinuteEvent(int minute, int hour)
    {
        timeText.text = $"{hour:00}:{minute:00}";
    }
}