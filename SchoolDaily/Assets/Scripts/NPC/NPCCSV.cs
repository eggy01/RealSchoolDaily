using System.Collections.Generic;
using UnityEngine;

public static class NPCCSVLoader
{
    public static List<NPCData> LoadNPCData(string csvFileName)
    {
        List<NPCData> npcList = new List<NPCData>();

        TextAsset csvData = Resources.Load<TextAsset>(csvFileName);
        if (csvData == null)
        {
            Debug.LogError($"CSV文件 {csvFileName} 未找到！");
            return npcList;
        }

        string[] lines = csvData.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] fields = line.Split(',');

            if (fields.Length < 8)
            {
                Debug.LogWarning($"第{i}行数据不完整，已跳过");
                continue;
            }

            NPCData npc = new NPCData
            {
                NPCID = fields[0],
                NPCName = fields[1],
                NPCCpllege = fields[2],
                NPCMajor = fields[3],
                NPCBirthday = fields[4],
                NPCSkill = fields[5],
                NPCIconPath = fields[6],
                NPCDirectPath = fields[7]
            };

            npcList.Add(npc);
        }

        return npcList;
    }
}