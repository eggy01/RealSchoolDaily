using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class CSVLoader
{
    public static List<ItemData> LoadItemData(string csvFileName)
    {
        List<ItemData> itemList = new List<ItemData>();

        TextAsset csvData = Resources.Load<TextAsset>(csvFileName);
        if (csvData == null)
        {
            Debug.LogError($"CSV文件 {csvFileName} 未找到！");
            return itemList;
        }

        string[] lines = csvData.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] fields = line.Split(',');

            // 列数为12行
            if (fields.Length < 12)
            {
                Debug.LogWarning($"第{i}行数据不完整，已跳过");
                continue;
            }

            ItemData item = new ItemData
            {
                ID = fields[0],
                Name = fields[1],
                Type = fields[2],
                Collect = ParseBool(fields[3]),
                Abandon = ParseBool(fields[4]),
                Present = ParseBool(fields[5]),
                Size = int.Parse(fields[6]),
                Price = int.Parse(fields[7]),
                Use = fields[8],
                Describe = fields[9],
                ShopTypes = ParseShopTypes(fields[10]),
                IconPath = fields[11]
            };

            itemList.Add(item);
        }

        return itemList;
    }

    private static bool ParseBool(string value)
    {
        return value.Trim().ToLower() == "true";
    }
    private static string[] ParseShopTypes(string value)
    {
        return value.Split('/')
                   .Select(s => s.Trim())
                   .Where(s => !string.IsNullOrEmpty(s))
                   .ToArray();
    }
}