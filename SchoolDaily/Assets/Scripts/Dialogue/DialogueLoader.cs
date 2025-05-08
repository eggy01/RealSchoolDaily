using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.PlayerLoop;

public class DialogueLoader : MonoBehaviour
{
    public static DialogueLoader Instance { get; private set; }

    // 当前可用的剧情文件夹（根据日期）
    public string ActiveFolder { get; private set; }

    // 常驻剧情文件夹名
    private const string PERSISTENT_FOLDER = "Persist";
    private TimeManager timeManager;

    // 配置每个剧情时间段
    private readonly Dictionary<string, DateRange> _dialogueSchedule = new()
    {
        { "Beginner",    new DateRange(new GameDate(9, 3), new GameDate(9, 4)) },  // 新手剧情
        { "Semester1/default",   new DateRange(new GameDate(9, 5), new GameDate(9, 22)) }, // 正常剧情
        { "Semester1/sx",new DateRange(new GameDate(9, 23), new GameDate(12, 24)) }// 失序剧情
    };

    private void Awake()
    {
        Instance = this;
        // timeManager = FindObjectOfType<TimeManager>();

    }
    public GameDate ParseDateString(string dateStr)
    {
        //Debug.Log("传入的日期字符");
        Match match = Regex.Match(dateStr, @"(\d+)月(\d+)日");
        if (match.Success && match.Groups.Count == 3)
        {
            int month = int.Parse(match.Groups[1].Value);
            int day = int.Parse(match.Groups[2].Value);
            //Debug.Log("处理的日期字符" + month + day);
            return new GameDate(month, day);
        }

        // Debug.LogError($"日期格式错误: {dateStr}，使用默认日期(9月1日)");
        return new GameDate(9, 1);
    }

    public void UpdateActiveFolder(GameDate currentDate)
    {
        foreach (var entry in _dialogueSchedule)
        {
            if (entry.Value.Contains(currentDate))
            {
                ActiveFolder = entry.Key;
                //Debug.Log($"当前日期{currentDate}匹配到剧情文件夹: {ActiveFolder}");
                return;
            }
        }

        ActiveFolder = null;
        //Debug.Log($"当前日期{currentDate}没有匹配的剧情文件夹");
    }

    public void RefreshAvailableDialogue()
    {
        var current = new GameDate(
            TimeManager.Instance.GetMonth(),
            TimeManager.Instance.GetDay()
        );
        UpdateActiveFolder(current);
    }

    public TextAsset LoadCSVFromResources(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("文件名不能为空");
            return null;
        }
        // 调试输出当前可用文件夹
        // Debug.Log($"尝试加载文件: {fileName}，当前ActiveFolder: {ActiveFolder ?? "null"}");

        // 1. 先尝试从日期匹配的文件夹加载
        if (!string.IsNullOrEmpty(ActiveFolder))
        {
            string path = $"DialogueCSV/{ActiveFolder}/{fileName}";
            var csv = Resources.Load<TextAsset>(path);

            if (csv != null)
            {
                //Debug.Log($"从日期文件夹[{ActiveFolder}]加载: {fileName}");
                return csv;
            }
            else
            {
                //Debug.Log($"在文件夹[{ActiveFolder}]中未找到: {fileName}");
            }
        }

        // 2. 回退到Persist文件夹加载
        string persistentPath = $"DialogueCSV/{PERSISTENT_FOLDER}/{fileName}";
        var persistentCsv = Resources.Load<TextAsset>(persistentPath);

        if (persistentCsv != null)
        {
            // Debug.Log($"从常驻文件夹[{PERSISTENT_FOLDER}]加载: {fileName}");
            return persistentCsv;
        }

        //Debug.LogError($"文件加载失败: 在日期文件夹[{ActiveFolder}]和常驻文件夹中均未找到 {fileName}");
        return null;
    }

    // 辅助结构
    public struct GameDate
    {
        public int Month;
        public int Day;

        public GameDate(int month, int day)
        {
            Month = month;
            Day = day;
        }

        public override string ToString() => $"{Month}月{Day}日";
    }

    private class DateRange
    {
        private readonly GameDate _start;
        private readonly GameDate _end;

        public DateRange(GameDate start, GameDate end)
        {
            _start = start;
            _end = end;
        }

        public bool Contains(GameDate date)
        {
            if (date.Month < _start.Month || date.Month > _end.Month)
                return false;

            if (date.Month == _start.Month && date.Day < _start.Day)
                return false;

            if (date.Month == _end.Month && date.Day > _end.Day)
                return false;

            return true;
        }
    }
}