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
public class NPCManager : MonoBehaviour
{
    public List<NPCLocalItem> npcs = new List<NPCLocalItem>();
    public static UnityEvent onNPCDataChanged = new UnityEvent();
    private static NPCManager _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public static NPCManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<NPCManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("NPCManager");
                    _instance = obj.AddComponent<NPCManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }
    public void Start()
    {
        // MeetNPC("101011831");
        // MeetNPC("101012007");
        // AddMuddledness("101012007", 20);
        // AddMuddledness("101011831", 10);

    }

    // 保存NPC数据
    public void SaveNPCData()
    {
        GameData tempData = SaveManager.Instance.GetTempData();
        if (tempData != null)
        {
            tempData.npcLocalItems = new List<NPCLocalItem>(npcs);
            Debug.Log("NPC数据已保存到临时存档");
        }
    }

    // 从存档系统加载NPC数据
    public void LoadNPCData()
    {
        GameData tempData = SaveManager.Instance.GetTempData();
        if (tempData != null && tempData.npcLocalItems != null)
        {
            npcs = new List<NPCLocalItem>(tempData.npcLocalItems);
            Debug.Log("NPC数据已从存档加载");
        }
        else
        {
            npcs = new List<NPCLocalItem>();
        }
        onNPCDataChanged.Invoke();
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
        SaveNPCData();
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
        SaveNPCData();
    }

    // 获取NPC数据
    public NPCLocalItem GetNPCData(string npcID)
    {
        return npcs.Find(n => n.NPCID == npcID);
    }
}