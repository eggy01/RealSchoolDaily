using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance; // 单例实例
    public event Action<int> OnHourChanged; // 小时变更事件 // 小时变更事件

    private int gameMinute, gameHour, gameDay, gameMonth, gameYear;
    private int gameWeekDay = 1; // 1-7表示周一到周日
    private int gameWeekCount = 1; // 学期周数计数器
    private int termCount = 1; // 学期计数器
    private bool isInTerm = false; // 是否在学期内
    private Season gameSeason;
    public bool gameClockPause;

    private float minuteTimer;
    private int lastUpdatedMinute = -1; // 记录上次更新的分钟数

    private readonly int[] monthDays = { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start() => NewGameTime();

    // 天气相关
    private bool dayChanged = false;

    public int GetMonth() => gameMonth;
    public int GetDay() => gameDay;
    public Season GetSeason() => gameSeason;
    public bool GetisInTerm() => isInTerm;
    public bool GetdayChanged() => dayChanged;

    public int GetHour() => gameHour;
    public int GetMinute() => gameMinute;

    private void SkipToNextMonth()
    {
        HandleMonthIncrement(); // 直接进入下个月
        EventHandler.CallGameDateEvent(gameHour, gameDay, gameMonth, gameYear,
                                     gameSeason, gameWeekDay, termCount);
    }

    private void Update()
    {
        // 测试专用：按 D 键跳到下一天（仅在 Unity 编辑器运行时可使用）
        if (Input.GetKeyDown(KeyCode.R))
        {
            SkipToNextDay();
        }

        // 测试
        if (Input.GetKeyDown(KeyCode.M))
        {
            SkipToNextMonth(); // 按 M 跳到下个月
        }

        if (!gameClockPause)
        {
            minuteTimer += Time.deltaTime;

            // 现实1秒=游戏1分钟
            if (minuteTimer >= Settings.minuteThreshold)
            {
                minuteTimer -= Settings.minuteThreshold;
                UpdateGameTime();
            }
        }
    }

    private void NewGameTime()
    {
        gameMinute = 0;
        gameHour = 7;
        gameDay = 1;
        gameMonth = 2; // 直接从2月1日(周一)开始
        gameYear = 0;
        gameWeekDay = 1; // 周一
        gameWeekCount = 1;
        termCount = 1; // 第一学期
        isInTerm = true;
        UpdateSeason();

        // 初始化时发送一次事件
        EventHandler.CallGameMinuteEvent(gameMinute, gameHour);
        EventHandler.CallGameDateEvent(gameHour, gameDay, gameMonth, gameYear,
                                     gameSeason, gameWeekDay, termCount);
    }

    private void UpdateGameTime()
    {
        gameMinute++;

        // 检查是否到达整点
        if (gameMinute > Settings.minuteHold)
        {
            gameMinute = 0;
            if (++gameHour > Settings.hourHold)
            {
                gameHour = 0;

                HandleDayIncrement();
            }

            // 触发小时变更事件
            OnHourChanged?.Invoke(gameHour);

            // 整点强制更新为00
            EventHandler.CallGameMinuteEvent(0, gameHour);
            lastUpdatedMinute = 0;

            // 如果是通过分钟更新进入整点，则直接返回，避免后续逻辑
            if (lastUpdatedMinute != 0)
            {
                return;
            }
        }
        else
        {
            // 非整点时的10分钟更新逻辑
            if (gameMinute / 10 != lastUpdatedMinute / 10)
            {
                lastUpdatedMinute = gameMinute;
                EventHandler.CallGameMinuteEvent(gameMinute, gameHour);
            }
        }
    }

    private void HandleDayIncrement()
    {
        // 天气相关
        dayChanged = true; // 标记新的一天开始

        gameDay++;

        // 天气相关
        Debug.Log($"日期变化！当前天数：{gameDay}"); // 添加调试日志
        EventHandler.CallOnDayChangedEvent();
        gameWeekDay = gameWeekDay % 7 + 1; // 更新星期

        if (isInTerm) gameWeekCount++; // 学期内周数累计

        // 检查月份结束
        int currentMonthDays = GetMonthDays(gameMonth, gameYear);
        if (gameDay > currentMonthDays)
        {
            gameDay = 1;
            HandleMonthIncrement();
        }

        EventHandler.CallGameDateEvent(gameHour, gameDay, gameMonth, gameYear,
                                     gameSeason, gameWeekDay, termCount);
        CheckDateChange();
    }

    private void HandleMonthIncrement()
    {
        gameMonth++;
        if (gameMonth > 12)
        {
            gameMonth = 1;
            gameYear++;
        }

        // 检查学期切换
        if (gameMonth == 7) // 6月结束进入暑假
        {
            isInTerm = false;
            SkipToMonth(9); // 直接跳到9月
            termCount++;
        }
        else if (gameMonth == 1) // 12月结束进入寒假
        {
            isInTerm = false;
            SkipToMonth(2); // 直接跳到2月
            termCount++;
            if (termCount > 8) Debug.Log("Game Over");
        }
        else if (gameMonth == 9 || gameMonth == 2) // 新学期开始
        {
            isInTerm = true;
            gameWeekCount = 1; // 重置周数计数
        }

        UpdateSeason();
    }

    private void SkipToMonth(int targetMonth)
    {
        while (gameMonth != targetMonth)
        {
            gameMonth++;
            if (gameMonth > 12)
            {
                gameMonth = 1;
                gameYear++;
            }
            UpdateSeason();
        }
    }

    private string _lastDate; // 记录上次的日期
    private void CheckDateChange()
    {
        var currentDate = GetCurrentDateTime();
        if (!currentDate.Equals(_lastDate))
        {
            _lastDate = currentDate;
            EventHandler.CallDateChangedEvent(currentDate);
            Debug.Log("日期已变更: " + currentDate);
        }
    }
    public void SkipToNextDay()//跳到第二天
    {
        gameHour = 7;  // 强制设置为7点
        gameMinute = 0;
        minuteTimer = 0;
        lastUpdatedMinute = 0;

        gameDay++;
        gameWeekDay = gameWeekDay % 7 + 1;

        if (gameDay > GetMonthDays(gameMonth, gameYear))
        {
            gameDay = 1;
            HandleMonthIncrement();
        }

        EventHandler.CallGameMinuteEvent(gameMinute, gameHour);
        EventHandler.CallGameDateEvent(gameHour, gameDay, gameMonth, gameYear,
                                     gameSeason, gameWeekDay, termCount);
    }

    private void UpdateSeason()
    {
        gameSeason = (Season)((gameMonth - 1) / 3);
    }

    private int GetMonthDays(int month, int year)
    {
        if (month == 2 && IsLeapYear(year))
            return 29;
        return monthDays[month];
    }

    private bool IsLeapYear(int year)
    {
        return (year % 4 == 0 && year % 100 != 0) || year % 400 == 0;
    }

    /// <summary>
    /// 跳转到指定时间
    /// 支持格式：
    /// - "HH:mm" (如 "9:00")
    /// - "MM月dd日 HH:mm" (如 "2月1日 8:00")
    /// - "yyyy年MM月dd日 HH:mm" (如 "0年2月1日 8:00")
    /// </summary>
    public void SkipToTime(string timeString)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(timeString))
                throw new ArgumentException("时间字符串不能为空");

            // 解析纯时间格式（如 "9:00"）
            if (timeString.Contains(":") && !timeString.Contains("月") && !timeString.Contains("日"))
            {
                ParseTimeOnly(timeString);
            }
            // 解析带日期的格式（如 "2月1日 8:00"）
            else if (timeString.Contains("月") && timeString.Contains("日"))
            {
                ParseDateTime(timeString);
            }
            else
            {
                throw new FormatException("不支持的时间格式");
            }

            // 更新游戏时间并触发事件
            UpdateSeason();
            EventHandler.CallGameMinuteEvent(gameMinute, gameHour);
            EventHandler.CallGameDateEvent(gameHour, gameDay, gameMonth, gameYear,
                                         gameSeason, gameWeekDay, termCount);

            Debug.Log($"已跳转到时间: {gameYear}年{gameMonth}月{gameDay}日 {gameHour}:{gameMinute:D2}");
        }
        catch (Exception e)
        {
            Debug.LogError($"时间跳转失败: {e.Message}");
        }
    }

    // 解析纯时间格式（如 "9:00"）
    private void ParseTimeOnly(string timeString)
    {
        string[] timeParts = timeString.Split(':');
        if (timeParts.Length != 2)
            throw new FormatException("时间格式应为HH:mm");

        int hour = int.Parse(timeParts[0]);
        int minute = int.Parse(timeParts[1]);

        if (hour < 0 || hour > 23 || minute < 0 || minute > 59)
            throw new ArgumentOutOfRangeException("时间值超出范围");

        gameHour = hour;
        gameMinute = minute;
        minuteTimer = 0;
        lastUpdatedMinute = minute;
    }

    // 解析带日期的格式（如 "2月1日 8:00"）
    private void ParseDateTime(string dateTimeString)
    {
        // 分割日期和时间部分
        string[] dateTimeParts = dateTimeString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (dateTimeParts.Length != 2)
            throw new FormatException("日期时间格式应为'MM月dd日 HH:mm'");

        // 解析日期部分
        string datePart = dateTimeParts[0];
        int monthEnd = datePart.IndexOf('月');
        int dayEnd = datePart.IndexOf('日');

        if (monthEnd == -1 || dayEnd == -1)
            throw new FormatException("日期格式应为'MM月dd日'");

        int month = int.Parse(datePart.Substring(0, monthEnd));
        int day = int.Parse(datePart.Substring(monthEnd + 1, dayEnd - monthEnd - 1));

        // 检查月份和日期有效性
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException("月份必须在1-12之间");

        int maxDays = GetMonthDays(month, gameYear);
        if (day < 1 || day > maxDays)
            throw new ArgumentOutOfRangeException($"日期必须在1-{maxDays}之间");

        // 解析时间部分
        ParseTimeOnly(dateTimeParts[1]);

        // 更新日期
        gameMonth = month;
        gameDay = day;

        // 更新星期（需要根据日期计算）
        UpdateWeekDay();
    }

    // 根据日期更新星期几（简化版，实际应根据具体日历实现）
    private void UpdateWeekDay()
    {
        // 这里实现一个简单的星期计算逻辑
        // 实际项目中应该使用更精确的算法或查表法
        DateTime baseDate = new DateTime(2023, 2, 1); // 假设2023年2月1日是周三
        int baseWeekDay = 3;

        DateTime targetDate = new DateTime(gameYear + 2023, gameMonth, gameDay);
        int daysDiff = (targetDate - baseDate).Days;
        gameWeekDay = (baseWeekDay + daysDiff - 1) % 7 + 1;
    }

    // 添加获取完整日期时间的方法
    public string GetCurrentDateTime()
    {
        return (gameMonth + "月" + gameDay + "日");
    }
}