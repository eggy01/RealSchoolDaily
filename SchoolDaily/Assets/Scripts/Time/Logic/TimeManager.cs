using UnityEngine;

public class TimeManager : MonoBehaviour
{
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

    private void Start() => NewGameTime();

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

        // 测试
        if (Input.GetKeyDown(KeyCode.M))
        {
            SkipToNextMonth(); // 按 M 跳到下个月
        }

        //
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

        // 先检查是否到达整点（避免先显示01再跳转）
        if (gameMinute > Settings.minuteHold)
        {
            gameMinute = 0;
            if (++gameHour > Settings.hourHold)
            {
                gameHour = 0;
                HandleDayIncrement();
            }
            // 整点强制更新为00
            EventHandler.CallGameMinuteEvent(0, gameHour);
            lastUpdatedMinute = 0; // 重置最后更新分钟
            return; // 直接返回，避免后续逻辑
        }

        // 非整点时的10分钟更新逻辑
        if (gameMinute / 10 != lastUpdatedMinute / 10)
        {
            lastUpdatedMinute = gameMinute;
            EventHandler.CallGameMinuteEvent(gameMinute, gameHour);
        }
    }

    private void HandleDayIncrement()
    {
        gameDay++;
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

}