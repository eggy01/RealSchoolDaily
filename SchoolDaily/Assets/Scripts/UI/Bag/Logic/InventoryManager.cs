using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public List<ItemData> itemDatabase = new List<ItemData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        itemDatabase = CSVLoader.LoadItemData("ItemsData");
    }

    public ItemData GetItemByID(string id)
    {
        itemDatabase = CSVLoader.LoadItemData("ItemsData");
        foreach (var item in itemDatabase)
        {
            if (item.ID == id)
            {
                return item;
            }
        }

        Debug.LogWarning($"找不到ID为 {id} 的物品数据");
        return null;
    }
}
