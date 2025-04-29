using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(EventTrigger))]
public class ShopItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;

    [Header("选中颜色")]
    public Color normalNameColor;
    public Color selectedNameColor;
    public Color normalPriceColor;
    public Color selectedPriceColor;
    public Color highlightedColor;

    public ItemData Item { get; private set; }

    private bool isSelected;

    public void Initialize(ItemData item, Sprite iconSprite, string displayName, string price)
    {
        Item = item;
        icon.sprite = iconSprite;
        nameText.text = displayName;
        priceText.text = price;
        SetSelected(false);
        highlightedColor = new Color(227f/255f, 225f/255f, 160f/255f);
    }

    public void SetSelected(bool isSelected)
    {
        this.isSelected = isSelected;
        nameText.color = isSelected ? selectedNameColor : normalNameColor;
        priceText.color = isSelected ? selectedPriceColor : normalPriceColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        nameText.color = highlightedColor;
        priceText.color = highlightedColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isSelected)
        {
            nameText.color = selectedNameColor;
            priceText.color = selectedPriceColor;
        }
        else
        {
            nameText.color = normalNameColor;
            priceText.color = normalPriceColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SetSelected(!isSelected); // 切换选中状态
    }
}