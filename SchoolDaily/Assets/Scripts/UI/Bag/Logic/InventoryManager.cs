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
