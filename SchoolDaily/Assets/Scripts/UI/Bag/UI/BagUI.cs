// BagUI.cs （控制整个背包界面）
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class BagUI : MonoBehaviour
{
    public static BagUI Instance;
    [Header("UI组件")]
    public Transform contentParent; // ScrollView的Content对象
    public GameObject bagItemPrefab; // 物品预制体

    [Header("设置")]
    public Sprite defaultIcon; // 默认图标

    [Header("物品详情")]
    private Dictionary<GameObject, GameObject> itemDetailMap = new Dictionary<GameObject, GameObject>(); // 存储物品与对应详情面板的关系
    public GameObject detailPanelPrefab;

<<<<<<< Updated upstream
=======
    // 初始化方法
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        if (bagItemPrefab == null)
        {
            Debug.LogError("未绑定物品预制体！");
            enabled = false;
        }
        detelePrefab.SetActive(false);
        Mask1.SetActive(false);
        Mask2.SetActive(false);
    }
    public void ForceRefresh()
    {
        // 清理所有详情面板
        foreach (var pair in itemDetailMap)
        {
            Destroy(pair.Value);
        }
        itemDetailMap.Clear();

        // 重新生成背包物品
        RefreshBag();
    }
>>>>>>> Stashed changes
    private void OnEnable()
    {
        PackageLocalData.onInventoryChanged.AddListener(RefreshBag);
        ForceRefresh();
    }

    private void OnDisable()
    {
        PackageLocalData.onInventoryChanged.RemoveListener(RefreshBag);
        ForceRefresh();
    }
<<<<<<< Updated upstream

=======
    
    #region 刷新背包
>>>>>>> Stashed changes
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

            // 设置预制体UI
            itemUI.Setup(itemData, bagItem.Num, defaultIcon);

            // 添加点击监听
            Button itemBtn = newItem.GetComponent<Button>();
            if (itemBtn == null) itemBtn = newItem.AddComponent<Button>();
            itemBtn.onClick.AddListener(() =>
            {
                OnItemClick(newItem); // 调用物品点击处理方法

                // 调用 MarkAsRead 方法
                BagItemUI itemUI = newItem.GetComponent<BagItemUI>();
                if (itemUI != null)
                {
                    PackageLocalData.Instance.MarkAsRead(itemUI.GetItemData().ID, itemUI);
                }
            });
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

    #region 物品详情
    public void OnItemClick(GameObject clickedItem)
    {
        // 检查是否已存在该物品的详情面板
        if (itemDetailMap.TryGetValue(clickedItem, out GameObject existingPanel))
        {
            // 已存在则关闭
            CloseDetailPanel(clickedItem);
        }
        else
        {
            // 创建新面板
            CreateDetailPanel(clickedItem);
        }
    }

    private void CreateDetailPanel(GameObject item)
    {
        // 实例化面板并插入正确位置
        // 获取 Content 节点的引用
        Transform contentTransform = transform.Find("Bag/Right/Viewport/Content");
        // 实例化新面板，并将其父对象设置为 Content
        GameObject newPanel = Instantiate(detailPanelPrefab, contentTransform);
        newPanel.transform.SetSiblingIndex(item.transform.GetSiblingIndex() + 1);

        BagItemUI itemUI = item.GetComponent<BagItemUI>();
        DetailItemUI detailUI = newPanel.GetComponent<DetailItemUI>();
        if (itemUI && detailUI)
        {
            detailUI.Setup(itemUI.GetItemData());
        }

        // 存储关系
        itemDetailMap.Add(item, newPanel);
    }

    public void CloseDetailPanel(GameObject targetItem)
    {
        if (itemDetailMap.TryGetValue(targetItem, out GameObject panel))
        {
            Destroy(panel);
            itemDetailMap.Remove(targetItem);
        }
    }
    #endregion
}