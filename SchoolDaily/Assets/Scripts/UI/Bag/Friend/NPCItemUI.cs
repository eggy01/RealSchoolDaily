using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NPCItemUI : MonoBehaviour
{
    [Header("UI组件")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public Image favorIcon; // 好感度图标组件
    public TextMeshProUGUI favorText;
    public TextMeshProUGUI muddleText;
    public Slider muddleSlider;
    public Image tagImage; // tag图片组件
    public Sprite normlTag; // Normal状态的tag图片
    public Sprite selectTag; // Selected状态的tag图片

    [Header("好感度图标配置")]
    public Sprite[] favorIcons = new Sprite[6]; // 按顺序配置6个图标

    public void Setup(NPCData staticData, NPCLocalItem dynamicData, Sprite defaultIcon)
    {
        // 基础信息设置
        Sprite icon = Resources.Load<Sprite>(staticData.NPCIconPath);
        iconImage.sprite = icon != null ? icon : defaultIcon;
        nameText.text = staticData.NPCName;

        // 设置好感度和混乱值
        if (dynamicData != null)
        {
            UpdateFavorIcon(dynamicData.Favorability);
            muddleSlider.maxValue = 100;
            muddleSlider.value = dynamicData.NPCMuddledness;
            muddleText.text = $"{dynamicData.NPCMuddledness}%";
            favorText.text = $"{dynamicData.Favorability}";
        }
    }

    private void UpdateFavorIcon(int favorValue)
    {
        int iconIndex = CalculateFavorIconIndex(favorValue);
        if (iconIndex >= 0 && iconIndex < favorIcons.Length && favorIcons[iconIndex] != null)
        {
            favorIcon.sprite = favorIcons[iconIndex];
            favorIcon.gameObject.SetActive(true);
        }
        else
        {
            favorIcon.gameObject.SetActive(false);
        }
    }

    private int CalculateFavorIconIndex(int favor)
    {
        if (favor <= 0) return 0;    // 敌对
        if (favor < 20) return 1;     // 冷漠
        if (favor < 40) return 2;     // 普通
        if (favor < 80) return 3;     // 友好
        if (favor < 100) return 4;     // 亲密
        return 5;                     // 挚友（>=100）
    }

    // 切换tag图片状态
    public void SetTagActive(bool isActive)
    {
        if (tagImage != null)
        {
            tagImage.sprite = isActive ? selectTag : normlTag;
            tagImage.gameObject.SetActive(true);
        }
    }
}