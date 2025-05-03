using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

[Serializable]
public class NPCLocalItem
{
    public string NPCID;         // NPC唯一标识
    public bool IsMeet;          // 是否已遇见
    public int Favorability;     // 好感度（无下限-100）
    public int NPCMuddledness;   // 混乱值（0-100）

    public override string ToString()
    {
        return string.Format("[ID]:{0} [Meet]:{1} [Favorability]:{2} [NPCMuddledness]:{3}", 
            NPCID, IsMeet, Favorability, NPCMuddledness);
    }
}

[Serializable]
public class NPCManager
{
    public List<NPCLocalItem> npcs = new List<NPCLocalItem>();
    public static UnityEvent onNPCDataChanged = new UnityEvent();
    private static NPCManager _instance;

    public static NPCManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new NPCManager();
                _instance.LoadNPCData();
            }
            return _instance;
        }
    }

    // 保存NPC数据
    public void SaveNPCData()
    {
        string npcJson = JsonUtility.ToJson(this);
        PlayerPrefs.SetString("NPCLocalData", npcJson);
        PlayerPrefs.Save();
    }

    // 加载NPC数据
    public void LoadNPCData()
    {
        if (PlayerPrefs.HasKey("NPCLocalData"))
        {
            string npcJson = PlayerPrefs.GetString("NPCLocalData");
            JsonUtility.FromJsonOverwrite(npcJson, this);
        }
        else
        {
            npcs = new List<NPCLocalItem>();
        }
    }

    // 首次遇见NPC
    public void MeetNPC(string npcID)
    {
        var npc = npcs.Find(n => n.NPCID == npcID);
        if (npc == null)
        {
            npcs.Add(new NPCLocalItem
            {
                NPCID = npcID,
                IsMeet = true,
                Favorability = 0,
                NPCMuddledness = 0
            });
            SaveNPCData();
            onNPCDataChanged.Invoke();
        }
        else if (!npc.IsMeet)
        {
            npc.IsMeet = true;
            SaveNPCData();
            onNPCDataChanged.Invoke();
        }
    }

    // 增加好感度
    public void AddFavorability(string npcID, int amount)
    {
        var npc = npcs.Find(n => n.NPCID == npcID);
        if (npc != null)
        {
            npc.Favorability = Mathf.Clamp(npc.Favorability + amount, int.MinValue, 100);
            SaveNPCData();
            onNPCDataChanged.Invoke();
        }
    }

    // 减少好感度
    public void ReduceFavorability(string npcID, int amount)
    {
        AddFavorability(npcID, -amount);
    }

    // 增加混乱值
    public void AddMuddledness(string npcID, int amount)
    {
        var npc = npcs.Find(n => n.NPCID == npcID);
        if (npc != null)
        {
            npc.NPCMuddledness = Mathf.Clamp(npc.NPCMuddledness + amount, 0, 100);
            SaveNPCData();
            onNPCDataChanged.Invoke();
        }
    }

    // 减少混乱值
    public void ReduceMuddledness(string npcID, int amount)
    {
        AddMuddledness(npcID, -amount);
    }

    // 获取NPC数据
    public NPCLocalItem GetNPCData(string npcID)
    {
        return npcs.Find(n => n.NPCID == npcID);
    }
}