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
    public int MaxCapacity = 6400; // 背包总容量
    public static UnityEvent onInventoryChanged = new UnityEvent();
    private static PackageLocalData _instance;

    public static PackageLocalData Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new PackageLocalData();
            }
            return _instance;
        }
    }

    public void LoadData()
    {
        GameData tempData = SaveManager.Instance.GetTempData();

        // 确保无论如何都初始化容量值
        MaxCapacity = 6400; // 先设置默认值

        if (tempData != null)
        {
            // 优先使用存档中的容量值，如果没有则保持默认
            MaxCapacity = tempData.packageCapacity > 0 ? tempData.packageCapacity : 6400;

            if (tempData.packageItems != null)
            {
                items = new List<PackageLocalItem>(tempData.packageItems);
            }
        }

        Debug.Log($"最终背包容量: {MaxCapacity}"); // 调试用
        onInventoryChanged.Invoke();
    }
    private void SaveData()
    {
        GameData tempData = SaveManager.Instance.GetTempData();
        if (tempData != null)
        {
            tempData.packageItems = new List<PackageLocalItem>(items);
            tempData.packageCapacity = MaxCapacity; // 保存总容量
            Debug.Log("背包数据已保存到临时存档");
        }
    }


    // 添加获取新物品状态的方法
    public bool IsItemNew(string itemID)
    {
        PackageLocalItem item = items.Find(i => i.ID == itemID);
        return item != null && item.IsNew;
    }

    // 标记物品已读（点击后调用）
    public void MarkAsRead(string itemID, BagItemUI olditem)
    {
        olditem.newTag.SetActive(false);
        PackageLocalItem item = items.Find(i => i.ID == itemID);
        if (item != null)
        {
            item.IsNew = false;
            SaveData(); // 修改为调用统一保存方法
        }
    }


    // 计算当前已用容量
    public int CalculateTotalUsed()
    {
        int total = 0;
        foreach (var item in items)
        {
            ItemData data = InventoryManager.Instance.GetItemByID(item.ID);
            if (data != null) total += item.Num * data.Size;
        }
        return total;
    }

    public void AddCapacity(int value)
    {
        MaxCapacity += value;
        SaveData();
    }

    // 添加物品到背包
    public bool AddItem(string itemID, int amount = 1)
    {
        ItemData itemData = InventoryManager.Instance.GetItemByID(itemID);
        if (itemData == null) return false;

        int addedSize = itemData.Size * amount;
        if (CalculateTotalUsed() + addedSize > MaxCapacity)
        {
            Debug.Log("背包容量不足！");
            return false;
        }

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
        onInventoryChanged.Invoke(); // 触发全局事件
        SaveData();
        return true;
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
            onInventoryChanged.Invoke(); // 触发全局事件
            SaveData();
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

        Debug.Log($"背包中的物品数量:{items.Count}");
    }
    public void ForceRefresh()
    {
        onInventoryChanged.Invoke();
    }
}