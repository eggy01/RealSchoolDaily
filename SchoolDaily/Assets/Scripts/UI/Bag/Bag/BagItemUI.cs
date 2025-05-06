using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BagItemUI : MonoBehaviour
{
    private ItemData itemData;
    [Header("UI组件")]
    public Image iconImage;
    public Image imageName;
    public Sprite normlName;
    public Sprite selectName;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI countText;
    public TextMeshProUGUI sizeText;
    public TextMeshProUGUI idText;
    public GameObject newTag; // 可选的新物品提示

    [HideInInspector]
    public string ItemID; // ItemID

    public void Setup(ItemData item, int count, Sprite defaultIcon)
    {
        itemData = item;
        ItemID = item.ID; //存储 ItemID
        // 加载图标
        Sprite icon = Resources.Load<Sprite>(item.IconPath);
        iconImage.sprite = icon != null ? icon : defaultIcon;

        nameText.text = item.Name;
        countText.text = $"× {count}";
        sizeText.text = $"{item.Size} kb";
        idText.text = ItemID;
        // 显示新物品标识
        if (newTag != null)
            newTag.SetActive(PackageLocalData.Instance.IsItemNew(item.ID));
    }
    public ItemData GetItemData()
    {
        return itemData;
    }
}