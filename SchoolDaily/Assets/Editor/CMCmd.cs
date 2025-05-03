using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CMCmd : MonoBehaviour
{
    public class GMCmd
    {
        private static List<ItemData> itemDatabase;
         private static List<NPCData> NPCDatabase;

        [MenuItem("CMCmd/读取物品表格")]
        static void ReadTable()
        {
            itemDatabase = CSVLoader.LoadItemData("ItemsData");

            if (itemDatabase.Count > 0)
            {
                Debug.Log("所有商品的 ShopTypes");
                foreach (var item in itemDatabase)
                {
                    Debug.Log($"{item.ID}，商品: {item.Name}, ShopTypes: {string.Join(", ", item.ShopTypes)}");
                }
            }
            else
            {
                Debug.Log("未找到商品数据");
            }

        }
        [MenuItem("CMCmd/读取NPC表格")]
        static void NPCReadTable()
        {
            NPCDatabase = NPCCSVLoader.LoadNPCData("NPCData");

            if (NPCDatabase.Count > 0)
            {
                Debug.Log("所有NPC");
                foreach (var NPC in NPCDatabase)
                {
                    Debug.Log($"{NPC.NPCID}，姓名: {NPC.NPCName}");
                }
            }
            else
            {
                Debug.Log("未找到NPC数据");
            }

        }
    }
}
