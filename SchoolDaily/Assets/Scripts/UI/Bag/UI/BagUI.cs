// BagUI.cs （控制整个背包界面）
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BagUI : MonoBehaviour
{
    [Header("UI Components")]
    public Transform contentParent; // ScrollView的Content对象
    public GameObject bagItemPrefab; // 物品预制体

    [Header("Settings")]
    public Sprite defaultIcon; // 默认图标

    private void OnEnable()
    {
        // 注册数据变更事件
        PackageLocalData.onInventoryChanged.AddListener(RefreshBag);
        RefreshBag();
    }

    private void OnDisable()
    {
        PackageLocalData.onInventoryChanged.RemoveListener(RefreshBag);
    }

    public void RefreshBag()
    {
        //测试
        if (contentParent == null) Debug.LogError("contentParent 未赋值！");
        if (PackageLocalData.Instance == null) Debug.LogError("PackageLocalData 实例为 null");
        if (InventoryManager.Instance == null) Debug.LogError("InventoryManager 实例为 null");
        // 清空旧物品
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 获取背包数据
        List<PackageLocalItem> bagItems = PackageLocalData.Instance.LoadPackage();

        // 生成新物品列表
        foreach (PackageLocalItem bagItem in bagItems)
        {
            // 获取对应的ItemData
            ItemData itemData = InventoryManager.Instance.GetItemByID(bagItem.ID);
            if (itemData == null)
            {
                Debug.LogWarning($"找不到ID为 {bagItem.ID} 的物品数据");
                continue;
            }

            // 实例化预制体
            GameObject newItem = Instantiate(bagItemPrefab, contentParent);
            BagItemUI itemUI = newItem.GetComponent<BagItemUI>();

            // 设置UI显示
            if (itemUI != null)
            {
                itemUI.Setup(itemData, bagItem.Num, defaultIcon);
            }
            else
            {
                Debug.LogError("预制体缺少 BagItemUI 组件");
            }
        }
    }

    // 初始化方法（在编辑器绑定预制体）
    private void Start()
    {
        if (bagItemPrefab == null)
        {
            Debug.LogError("未绑定物品预制体！");
            enabled = false;
        }
    }
}