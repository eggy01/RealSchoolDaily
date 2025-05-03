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
}
