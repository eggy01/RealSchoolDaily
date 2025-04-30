using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(EventTrigger))]
public class ShopItemUI : MonoBehaviour, IPointerClickHandler
{
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public ItemData Item { get; private set; }

    private bool isSelected;

    public void Initialize(ItemData item, Sprite iconSprite, string displayName, string price)
    {
        Item = item;
        icon.sprite = iconSprite;
        nameText.text = displayName;
        priceText.text = price;
        SetSelected(false);
    }

    public void SetSelected(bool isSelected)
    {
        this.isSelected = isSelected;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SetSelected(!isSelected); // 切换选中状态
    }
}