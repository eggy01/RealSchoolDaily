// BagItemUI.cs （挂载到预制体上）
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BagItemUI : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI countText;
    public TextMeshProUGUI sizeText;
    public GameObject newTag; // 可选的新物品提示

    public void Setup(ItemData item, int count, Sprite defaultIcon)
    {
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
}