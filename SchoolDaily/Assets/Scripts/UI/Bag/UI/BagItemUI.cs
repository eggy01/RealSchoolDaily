using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BagItemUI : MonoBehaviour
{
    private ItemData itemData; 
    [Header("UI组件")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI countText;
    public TextMeshProUGUI sizeText;
    public GameObject newTag; // 可选的新物品提示

    public void Setup(ItemData item, int count, Sprite defaultIcon)
    {
        itemData = item;
        // 加载图标
        Sprite icon = Resources.Load<Sprite>(item.IconPath);
        iconImage.sprite = icon != null ? icon : defaultIcon;

        nameText.text = item.Name;
        countText.text = $"{count}";
        sizeText.text = $"{item.Size}kb";
        // 显示新物品标识
        if(newTag != null) 
            newTag.SetActive(PackageLocalData.Instance.IsItemNew(item.ID));
    }
    public ItemData GetItemData()
    {
        return itemData;
    }
}