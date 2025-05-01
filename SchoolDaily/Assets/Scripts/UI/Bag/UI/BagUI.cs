// BagUI.cs （控制整个背包界面）
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;


public class BagUI : MonoBehaviour
{
    public static BagUI Instance;
    [Header("UI组件")]
    public Transform contentParent; // ScrollView的Content对象
    public GameObject bagItemPrefab; // 物品预制体
    public GameObject detelePrefab; //删除弹窗
    public GameObject Mask1;
    public GameObject Mask2;
    public TextMeshProUGUI deleteNum;

    [Header("设置")]
    public Sprite defaultIcon; // 默认图标

    [Header("物品详情")]
    private Dictionary<GameObject, GameObject> itemDetailMap = new Dictionary<GameObject, GameObject>(); // 存储物品与对应详情面板的关系
    public GameObject detailPanelPrefab;

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

    #region 刷新背包
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
    #endregion

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
        Transform contentTransform = transform.Find("Bag/背包/Viewport/Content");
        // 实例化新面板，并将其父对象设置为 Content
        GameObject newPanel = Instantiate(detailPanelPrefab, contentTransform);
        newPanel.transform.SetSiblingIndex(item.transform.GetSiblingIndex() + 1);

        BagItemUI itemUI = item.GetComponent<BagItemUI>();
        DetailItemUI detailUI = newPanel.GetComponent<DetailItemUI>();
        if (itemUI && detailUI)
        {
            detailUI.Setup(itemUI.GetItemData());
        }

        ///获取关闭按钮和垃圾桶
        Button closeBtn = detailUI.transform.Find("close").GetComponent<Button>();
        Button binBtn = detailUI.transform.Find("bin").GetComponent<Button>();
        closeBtn.onClick.AddListener(() => CloseDetailPanel(item));
        binBtn.onClick.AddListener(() => DeteleItem(itemUI));

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

    #region 删除物品
    public void DeteleItem(BagItemUI item)
    {
        int itemCount = PackageLocalData.Instance.GetItemCount(item.ItemID);
        int Num = 1; // 每次打开弹窗时从1开始

        // 每次调用时先移除旧的监听器
        Button YesBtn = detelePrefab.transform.Find("Yes").GetComponent<Button>();
        Button NoBtn = detelePrefab.transform.Find("No").GetComponent<Button>();
        Button upBtn = detelePrefab.transform.Find("up").GetComponent<Button>();
        Button downBtn = detelePrefab.transform.Find("down").GetComponent<Button>();

        // 清空旧的监听器
        YesBtn.onClick.RemoveAllListeners();
        NoBtn.onClick.RemoveAllListeners();
        upBtn.onClick.RemoveAllListeners();
        downBtn.onClick.RemoveAllListeners();

        // 初始化UI状态
        detelePrefab.SetActive(true);
        Mask1.SetActive(true);
        Mask2.SetActive(true);
        deleteNum.text = $"丢弃 1 个"; // 显式设置为初始值

        // 数量更新方法
        System.Action updateDeleteDisplay = () =>
        {
            Num = Mathf.Clamp(Num, 1, itemCount);
            deleteNum.text = $"丢弃 {Num} 个";
        };

        //点击空白处关闭
        Button maskBtn = Mask1.GetComponent<Button>();
        Button maskBtn2 = Mask2.GetComponent<Button>();
        if (maskBtn == null) // 确保有Button组件
        {
            maskBtn = Mask1.AddComponent<Button>();
            maskBtn.transition = Selectable.Transition.None; // 禁用按钮过渡效果
        }
        if (maskBtn2 == null) // 确保有Button组件
        {
            maskBtn2 = Mask2.AddComponent<Button>();
            maskBtn2.transition = Selectable.Transition.None; // 禁用按钮过渡效果
        }

        // 清空旧监听器后添加新监听
        maskBtn.onClick.RemoveAllListeners();
        maskBtn.onClick.AddListener(() =>
        {
            detelePrefab.SetActive(false);
            Mask1.SetActive(false);
            Mask2.SetActive(false);
            Num = 1; // 重置删除数量
        });

        maskBtn2.onClick.RemoveAllListeners();
        maskBtn2.onClick.AddListener(() =>
        {
            detelePrefab.SetActive(false);
            Mask1.SetActive(false);
            Mask2.SetActive(false);
            Num = 1; // 重置删除数量
        });

        // 按钮监听
        upBtn.onClick.AddListener(() =>
        {
            Num++;
            updateDeleteDisplay();
        });

        downBtn.onClick.AddListener(() =>
        {
            Num--;
            updateDeleteDisplay();
        });

        YesBtn.onClick.AddListener(() =>
        {
            PackageLocalData.Instance.RemoveItem(item.ItemID, Num);
            detelePrefab.SetActive(false);
            Mask1.SetActive(false);
            Mask2.SetActive(false);
            Num = 1; // 重置删除数量
        });

        NoBtn.onClick.AddListener(() =>
        {
            detelePrefab.SetActive(false);
            Mask1.SetActive(false);
            Mask2.SetActive(false);
            Num = 1; // 重置删除数量
        });
    }
    #endregion
}