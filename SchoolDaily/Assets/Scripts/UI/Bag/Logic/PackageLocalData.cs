using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

[System.Serializable]
public class PackageLocalItem
{
    public string ID;        // 对应ItemData的ID
    public int Num;          // 物品数量
    public bool IsNew;       // 是否为新获得的物品

    public override string ToString()
    {
        return string.Format("[ID]:{0} [Num]:{1} [IsNew]:{2}", ID, Num, IsNew);
    }

}

[System.Serializable]
public class PackageLocalData
{
    public List<PackageLocalItem> items = new List<PackageLocalItem>(); // 直接初始化
    public static UnityEvent onInventoryChanged = new UnityEvent();
    private static PackageLocalData _instance;

    public static PackageLocalData Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new PackageLocalData();
                _instance.LoadPackage(); // 确保加载数据时列表已存在
            }
            return _instance;
        }
    }

    // 添加获取新物品状态的方法
    public bool IsItemNew(string itemID)
    {
        PackageLocalItem item = items.Find(i => i.ID == itemID);
        return item != null && item.IsNew;
    }

    // 标记物品已读（点击后调用）
    public void MarkAsRead(string itemID)
    {
        PackageLocalItem item = items.Find(i => i.ID == itemID);
        if (item != null)
        {
            item.IsNew = false;
            SavePackage();
        }
    }

    // 保存背包数据
    public void SavePackage()
    {
        string inventoryJson = JsonUtility.ToJson(this);
        PlayerPrefs.SetString("PackageLocalData", inventoryJson);
        PlayerPrefs.Save();
    }

    // 加载背包数据
    public List<PackageLocalItem> LoadPackage()
    {

        if (PlayerPrefs.HasKey("PackageLocalData"))
        {
            string inventoryJson = PlayerPrefs.GetString("PackageLocalData");
            PackageLocalData packageLocalData = JsonUtility.FromJson<PackageLocalData>(inventoryJson);
            items = packageLocalData.items ?? new List<PackageLocalItem>(); // 空值保护
        }
        else
        {
            items = new List<PackageLocalItem>();
        }
        PrintInventory(); // 加载数据后输出背包信息
        return items;
    }

    // 添加物品到背包
    public void AddItem(string itemID, int amount = 1)
    {
        var existingItem = items.Find(i => i.ID == itemID);
        if (existingItem != null)
        {
            existingItem.Num += amount;
        }
        else
        {
            items.Add(new PackageLocalItem
            {
                ID = itemID,
                Num = amount,
                IsNew = true
            });
        }
        SavePackage();
        onInventoryChanged.Invoke(); // 触发全局事件
    }

    // 从背包移除物品
    public void RemoveItem(string itemID, int amount = 1)
    {
        var itemToRemove = items.Find(i => i.ID == itemID);
        if (itemToRemove != null)
        {
            itemToRemove.Num -= amount;
            if (itemToRemove.Num <= 0)
            {
                items.Remove(itemToRemove);
            }
            SavePackage();
            onInventoryChanged.Invoke(); // 触发全局事件
        }
    }

    // 获取物品数量
    public int GetItemCount(string itemID)
    {
        var item = items.Find(i => i.ID == itemID);
        return item != null ? item.Num : 0;
    }

    // 输出背包中的物品信息
    public void PrintInventory()
    {
        if (items == null || items.Count == 0)
        {
            Debug.Log("背包为空！");
            return;
        }

        Debug.Log("背包中的物品：");
        foreach (PackageLocalItem item in items)
        {
            Debug.Log(item.ToString());
        }
    }
}