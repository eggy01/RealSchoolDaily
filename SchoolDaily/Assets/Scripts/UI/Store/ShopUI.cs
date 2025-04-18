using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ShopUI : MonoBehaviour
{
    #region Singleton
    public static ShopUI Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    #endregion

    #region Public Variables
    [Header("UI组件")]
    public GameObject shopPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI totalCostText;
    public TextMeshProUGUI quantityText;
    public Transform itemContainer;
    public GameObject itemPrefab;
    public Button buyButton;

    [Header("设置")]
    public int maxQuantity = 99;
    public int minQuantity = 0;
    public Color normalColor = Color.white;
    public Color insufficientColor = Color.red;
    public Sprite defaultIcon;

    [Header("长按设置")]
    public float initialDelay = 0.5f;
    public float accelerationInterval = 0.1f;
    public float accelerationRate = 1.5f;
    public float maxSpeedMultiplier = 10f;
    #endregion

    #region Private Variables
    private ItemData _currentSelectedItem;
    private int _currentQuantity;
    private bool _isHolding;
    private Coroutine _holdCoroutine;
    private ItemData _currentHoldingItem;
    private int _currentDeltaDirection;
    #endregion

    #region Core Methods

    //打开商店 在ShopNPC中调用，传入shopType
    public void ShowShop(string shopType = "超市")
    {
        titleText.text = shopType;
        shopPanel.SetActive(true);
        RefreshShopItems();
        UpdateUI();
    }

    //刷新商店列表
    private void RefreshShopItems()
    {
        ClearItemContainer();

        //筛选
        var validItems = InventoryManager.Instance.itemDatabase
            .Where(item => item.ShopTypes.Contains(titleText.text) && item.Price > 0)
            .ToList();

        foreach (var item in validItems)
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
        UpdateQuantityDisplay();
        UpdateTotalCost();
        UpdateGoldDisplay();
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
        totalCostText.text = $"总价: {total}";
        totalCostText.color = total > GetCurrentGold() ? insufficientColor : normalColor;
    }

    private void UpdateGoldDisplay()
    {
        goldText.text = $" {GetCurrentGold()}";
    }

    private void UpdateBuyButtonState()
    {
        bool canBuy = _currentSelectedItem != null
            && _currentQuantity > 0
            && GetCurrentGold() >= _currentSelectedItem.Price * _currentQuantity;

        buyButton.interactable = canBuy;
    }
    #endregion

    #region Utility Methods
    private int GetCurrentGold() => GoldManager.Instance.CurrentGold;

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

    public void CloseShop()
    {
        shopPanel.SetActive(false);
        _currentSelectedItem = null;
        _currentQuantity = 0;
        EndHolding();
    }
    #endregion

    #region 购买逻辑
    public void TryPurchase()
    {
        if (_currentSelectedItem == null || _currentQuantity <= 0) return;

        int totalCost = _currentSelectedItem.Price * _currentQuantity;
        if (GoldManager.Instance.TrySpendGold(totalCost))
        {
            PackageLocalData.Instance.AddItem(_currentSelectedItem.ID, _currentQuantity);
            UpdateUI();
        }
    }
    #endregion
}