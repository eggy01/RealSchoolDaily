using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class RadarTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public PlayerInformation playerInfo;
    public GameObject tooltipPanel; // 包含所有文本的父物体
    public TextMeshProUGUI[] attributeTexts; // 按顺序：声望、道德、智力、理解力、天赋、社交


    private CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = tooltipPanel.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
    }

    void Update()
    {
        if (tooltipPanel.activeSelf)
        {
            canvasGroup.alpha = 1;
        }
        else
        {
            canvasGroup.alpha = 0;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UpdateTextValues();
        tooltipPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipPanel.SetActive(false);
    }

    void UpdateTextValues()
    {
        attributeTexts[0].text = $"声望: {playerInfo.CurrentData.fame}";
        attributeTexts[1].text = $"道德: {playerInfo.CurrentData.morality}";
        attributeTexts[1].color = playerInfo.CurrentData.morality < 0 ? Color.red : Color.black;
        attributeTexts[2].text = $"智力: {playerInfo.CurrentData.intelligence}";
        attributeTexts[3].text = $"理解: {playerInfo.CurrentData.comprehension}";
        attributeTexts[4].text = $"才艺: {playerInfo.CurrentData.talent}";
        attributeTexts[5].text = $"社交: {playerInfo.CurrentData.society}";
    }
}