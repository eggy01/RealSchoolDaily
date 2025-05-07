using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class ShopUI : MonoBehaviour, IWindow
{
    #region 单例
    public static ShopUI Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        close.onClick.AddListener(() => WindowManager.Instance.CloseWindow(this));
    }
    #endregion

    #region 公共变量
    public bool ShouldPauseTime => false;
    public bool ShouldPausePlayer => true;
    public bool IsOpen => shopPanel.activeSelf;
    [Header("UI组件")]
    public Button close;
    public GameObject shopPanel;
    public GameObject SurePanel; //提示框
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI totalCostText;
    public TextMeshProUGUI quantityText;
    public Transform itemContainer;
    public GameObject itemPrefab;
    public Button buyButton;
    public GameObject currenticonimage;
    public GameObject currentnameimage;
    public Image currenticon;
    public TextMeshProUGUI currentname;

    [Header("设置")]
    public int maxQuantity = 99;
    public int minQuantity = 0;
    public Color normalColor = Color.black;
    public Color insufficientColor = Color.red;
    public Sprite defaultIcon;
    private bool isPaused;

    [Header("长按设置")]
    public float initialDelay = 0.5f;
    public float accelerationInterval = 0.1f;
    public float accelerationRate = 1.5f;
    public float maxSpeedMultiplier = 10f;
    #endregion

    #region 私有变量
    private ItemData _currentSelectedItem;
    private int _currentQuantity;
    private bool _isHolding;
    private Coroutine _holdCoroutine;
    private ItemData _currentHoldingItem;
    private int _currentDeltaDirection;
    #endregion

    #region Core Methods

    public void Open(params object[] args)
    {
        // 参数解析
        string shopType = args.Length > 0 ? (string)args[0] : "超市";
        List<ItemData> customItems = args.Length > 1 ? (List<ItemData>)args[1] : null;

        // 核心打开逻辑
        shopPanel.SetActive(true);
        titleText.text = shopType;

        // 根据参数刷新商品
        if (customItems != null)
        {
            RefreshShopItems(customItems);
        }
        else
        {
            RefreshShopItems(InventoryManager.Instance.itemDatabase
                .Where(item => item.ShopTypes.Contains(shopType))
                .ToList());
        }

        // 重置购买状态
        _currentSelectedItem = null;
        _currentQuantity = 0;
        UpdateUI();
    }

    public void Close()
    {
        shopPanel.SetActive(false);
        SurePanel.SetActive(false);

        // 清理临时数据
        _currentSelectedItem = null;
        _currentQuantity = 0;
        ClearItemContainer();
    }

    //刷新商店列表
    private void RefreshShopItems(List<ItemData> items)
    {
        ClearItemContainer();

        foreach (var item in items.Where(i => i.Price > 0))
        {
            CreateShopItem(item);
        }
    }

    //创建商品列表
    private void CreateShopItem(ItemData item)
    {
        GameObject newItem = Instantiate(itemPrefab, itemContainer);//ShopItemUI中的方法
        SetupItemComponents(newItem, item);
    }

    private void SetupItemComponents(GameObject itemObj, ItemData item)
    {
        ShopItemUI ui = itemObj.GetComponent<ShopItemUI>();
        if (ui == null) ui = itemObj.AddComponent<ShopItemUI>();

        // 初始化UI组件 预制体
        ui.Initialize(item,
            Resources.Load<Sprite>(item.IconPath) ?? defaultIcon,
            item.Name,
            $"￥{item.Price}");

        // 配置事件触发器
        EventTrigger trigger = itemObj.GetComponent<EventTrigger>();
        if (trigger == null) trigger = itemObj.AddComponent<EventTrigger>();

        // 移除旧事件
        trigger.triggers.Clear();

        // 添加新事件
        AddTriggerEvent(trigger, EventTriggerType.PointerDown,
            data => OnPointerDown(item, (PointerEventData)data));
        AddTriggerEvent(trigger, EventTriggerType.PointerUp,
            data => OnPointerUp());
        AddTriggerEvent(trigger, EventTriggerType.PointerExit,
            data => OnPointerUp());
    }
    #endregion

    #region 长按
    private void OnPointerDown(ItemData item, PointerEventData eventData)
    {
        // 确定操作方向
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            _currentDeltaDirection = 1;
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            _currentDeltaDirection = -1;
        }
        else return;

        StartHolding(item);
        ModifyQuantity(_currentDeltaDirection); // 立即响应第一次点击
    }

    private void StartHolding(ItemData item)
    {
        _currentHoldingItem = item;
        _isHolding = true;
        if (_holdCoroutine != null) StopCoroutine(_holdCoroutine);
        _holdCoroutine = StartCoroutine(HoldAccelerationRoutine());
    }

    private void OnPointerUp()
    {
        EndHolding();
    }

    private void EndHolding()
    {
        _isHolding = false;
        _currentHoldingItem = null;
        if (_holdCoroutine != null)
        {
            StopCoroutine(_holdCoroutine);
            _holdCoroutine = null;
        }
    }

    private IEnumerator HoldAccelerationRoutine()
    {
        yield return new WaitForSecondsRealtime(initialDelay);

        float speedMultiplier = 1f;
        while (_isHolding)
        {
            int delta = _currentDeltaDirection * (int)speedMultiplier;
            ModifyQuantity(delta);

            speedMultiplier = Mathf.Clamp(
                speedMultiplier * accelerationRate,
                1f,
                maxSpeedMultiplier
            );

            yield return new WaitForSecondsRealtime(accelerationInterval);
        }
    }

    private void ModifyQuantity(int delta)
    {
        // 切换商品时重置数量
        if (_currentHoldingItem != _currentSelectedItem)
        {
            _currentQuantity = 0;
            _currentSelectedItem = _currentHoldingItem;
        }

        _currentQuantity = Mathf.Clamp(
            _currentQuantity + delta,
            minQuantity,
            maxQuantity
        );

        UpdateUI();
        UpdateSelectionVisual();
    }

    private void UpdateSelectionVisual()
    {
        foreach (Transform child in itemContainer)
        {
            ShopItemUI ui = child.GetComponent<ShopItemUI>();
            if (ui != null)
            {
                ui.SetSelected(ui.Item == _currentSelectedItem);
            }
        }
    }
    #endregion

    #region UI Updates
    private void UpdateUI()
    {
        Updatecurrenticon();
        Updatecurrentname();
        UpdateQuantityDisplay();
        UpdateTotalCost();
        UpdateBuyButtonState();
    }

    private void UpdateQuantityDisplay()
    {
        quantityText.text = _currentSelectedItem != null
            ? $"数量: {_currentQuantity}"
            : " ";
    }

    private void UpdateTotalCost()
    {
        int total = _currentSelectedItem?.Price * _currentQuantity ?? 0;
        totalCostText.text = _currentSelectedItem != null
            ? $"总价: {total}"
            : " ";
        totalCostText.color = total > GetCurrentGold() ? insufficientColor : normalColor;
    }

    private void UpdateBuyButtonState()
    {
        bool canBuy = _currentSelectedItem != null
            && _currentQuantity > 0
            && GetCurrentGold() >= _currentSelectedItem.Price * _currentQuantity;

        buyButton.interactable = canBuy;
    }
    private void Updatecurrenticon()
    {
        bool hasSelected = _currentSelectedItem != null;
        currenticonimage.SetActive(hasSelected);

        if (hasSelected)
        {
            Sprite itemIcon = Resources.Load<Sprite>(_currentSelectedItem.IconPath);
            currenticon.sprite = itemIcon ?? defaultIcon;
        }
        else
        {
            currenticon.sprite = defaultIcon; // 重置为默认图标
        }
    }

    private void Updatecurrentname()
    {
        bool hasSelected = _currentSelectedItem != null;
        currentnameimage.SetActive(hasSelected);

        currentname.text = hasSelected ? _currentSelectedItem.Name : "";
    }
    #endregion

    #region Utility Methods
    private int GetCurrentGold() => PlayerInformation.Instance.CurrentGold;

    private void ClearItemContainer()
    {
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void AddTriggerEvent(EventTrigger trigger, EventTriggerType type,
        UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }


    #endregion

    #region 购买逻辑
    public void TryPurchase()
    {
        if (_currentSelectedItem == null || _currentQuantity <= 0) return;

        // 首先尝试将物品添加到背包
        bool capacityAvailable = PackageLocalData.Instance.AddItem(_currentSelectedItem.ID, _currentQuantity);

        // 如果背包容量足够
        if (capacityAvailable)
        {
            int totalCost = _currentSelectedItem.Price * _currentQuantity;
            // 扣除金币
            PlayerInformation.Instance.TrySpendGold(totalCost);
        }
        else
        {
            // 如果背包容量不足，显示确认面板
            SurePanel.SetActive(true);
        }

        // 重置选择状态
        _currentSelectedItem = null;
        _currentQuantity = 0;

        UpdateUI();
        UpdateSelectionVisual();
    }
    #endregion
}