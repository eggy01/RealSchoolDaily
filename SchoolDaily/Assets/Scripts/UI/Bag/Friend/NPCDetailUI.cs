using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NPCDetailUI : MonoBehaviour
{
    [Header("UI组件")]
    public Image npcDirectImage;    // 立绘
    public TextMeshProUGUI idText;  // 学号
    public TextMeshProUGUI collegeText; 
    public TextMeshProUGUI majorText;
    public TextMeshProUGUI birthdayText;
    public TextMeshProUGUI skillText;
    

    public void Setup(NPCData staticData, NPCLocalItem dynamicData)
    {
        // 加载立绘
        Sprite direct = Resources.Load<Sprite>(staticData.NPCDirectPath);
        npcDirectImage.sprite = direct;

        // 设置静态信息
        idText.text = $"学号: {staticData.NPCID}";
        collegeText.text = $"学院: {staticData.NPCCpllege}";
        majorText.text = $"专业: {staticData.NPCMajor}";
        birthdayText.text = $"生日: {staticData.NPCBirthday}";
        skillText.text = $"能力: {staticData.NPCSkill}";
    }
}