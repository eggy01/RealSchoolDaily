using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DetailItemUI : MonoBehaviour
{
    [Header("UI组件")]
    public TextMeshProUGUI itemUse;
    public TextMeshProUGUI itemDescription;
    public Image itemIcon;

    public void Setup(ItemData data)
    {
        itemUse.text = $"{data.Use}";
        itemDescription.text = $"{data.Describe}";

        // 加载图标
        Sprite icon = Resources.Load<Sprite>(data.IconPath);
        itemIcon.sprite = icon;
    }
}