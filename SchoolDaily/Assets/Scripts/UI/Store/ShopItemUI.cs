using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(EventTrigger))]
public class ShopItemUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;

    [Header("选中颜色")]
    public Color normalNameColor = Color.black;
    public Color selectedNameColor = Color.yellow;
    public Color normalPriceColor = Color.gray;
    public Color selectedPriceColor = new Color(0.9f, 0.8f, 0.1f);

    public ItemData Item { get; private set; }

    public void Initialize(ItemData item, Sprite iconSprite, string displayName, string price)
    {
        Item = item;
        icon.sprite = iconSprite;
        nameText.text = displayName;
        priceText.text = price;
        SetSelected(false); // 初始化状态
    }

    public void SetSelected(bool isSelected)
    {
        // 通过文字颜色变化实现选中反馈
        nameText.color = isSelected ? selectedNameColor : normalNameColor;
        priceText.color = isSelected ? selectedPriceColor : normalPriceColor;
        
        
    }
}
