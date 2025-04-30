using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DetailItemUI : MonoBehaviour
{
    [Header("UI组件")]
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemSize;
    public TextMeshProUGUI itemUse;
    public TextMeshProUGUI itemDescription;
    public TextMeshProUGUI itemShopTypes;
    public Image itemIcon;

    public void Setup(ItemData data)
    {
        string ShopTypes = string.Join(" ", data.ShopTypes);
        itemName.text = data.Name;
        itemSize.text = $"存储：{data.Size}kb";
        itemUse.text = $"作用：{data.Use}";
        itemDescription.text = $"描述：{data.Describe}";
        itemShopTypes.text = $"获取途径：{ShopTypes}";

        // 加载图标
        Sprite icon = Resources.Load<Sprite>(data.IconPath);
        itemIcon.sprite = icon;
    }
}