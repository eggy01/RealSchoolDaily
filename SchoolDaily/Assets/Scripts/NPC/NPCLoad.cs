using System.Collections.Generic;
using UnityEngine;

public class NPCLoad : MonoBehaviour
{
    public static NPCLoad Instance;
    public List<NPCData> npcDatabase = new List<NPCData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadNPCData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadNPCData()
    {
        npcDatabase = NPCCSVLoader.LoadNPCData("NPCData");
    }

    public NPCData GetNPCByID(string id)
    {
        return npcDatabase.Find(n => n.NPCID == id);
    }

    // 新增：通过NPCName获取NPCID
    public string GetNPCIDByName(string name)
    {
        NPCData npc = npcDatabase.Find(n => n.NPCName == name);
        return npc != null ? npc.NPCID : null;
    }
}
