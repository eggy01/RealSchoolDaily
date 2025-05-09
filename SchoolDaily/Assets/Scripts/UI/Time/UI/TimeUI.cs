using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TimeUI : MonoBehaviour
{
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI weekDayText;
    public TextMeshProUGUI termText;
    public GameObject lifeMoney;

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
        if (day == 1)
        {
            PlayerInformation.Instance.AddGold(1500);
            lifeMoney.SetActive(true);
        }
    }

    private void OnGameMinuteEvent(int minute, int hour)
    {
        timeText.text = $"{hour:00}:{minute:00}";
    }
}