using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SimpleCSVParser
{
    public static List<Dictionary<string, string>> Parse(TextAsset csv)
    {
        var data = new List<Dictionary<string, string>>();
        string[] lines = csv.text.Split('\n');

        if (lines.Length < 2) return data;

        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            if (values.Length == 0 || string.IsNullOrEmpty(values[0])) continue;

            var entry = new Dictionary<string, string>();
            for (int j = 0; j < headers.Length && j < values.Length; j++)
            {
                entry[headers[j].Trim()] = values[j].Trim();
            }
            data.Add(entry);
        }

        return data;
    }
}
