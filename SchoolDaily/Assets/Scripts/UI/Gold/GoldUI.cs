using TMPro;
using UnityEngine;

public class GoldUI : MonoBehaviour
{
    [Header("组件")]
    public TextMeshProUGUI goldText;
    public Animator goldAnimator;
    
    [Header("动画参数")]
    public string addTrigger = "Add";
    public string spendTrigger = "Spend";

    void Start()
    {
        GoldManager.Instance.OnGoldUpdated.AddListener(UpdateGoldDisplay);
        UpdateGoldDisplay(GoldManager.Instance.CurrentGold);
    }

    void OnDestroy()
    {
        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.OnGoldUpdated.RemoveListener(UpdateGoldDisplay);
        }
    }

    void UpdateGoldDisplay(int amount)
    {
        goldText.text = $"{amount}";
        
        // 根据最近操作播放动画
        // （需要在其他逻辑中记录最近操作类型）
    }

    // 可扩展添加金币变化特效
}