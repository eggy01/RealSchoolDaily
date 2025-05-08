using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SaveSlotButton : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI slotText;      // 拖入子Text对象
    public GameObject Empty;

    public void SetSlotInfo(int slotIndex, bool isEmpty, GameTimeData timeData = null)
    {
        Debug.LogWarning("该槽是否为空：isEmpty" + isEmpty);
        if (isEmpty)
        {
            slotText.text = "NULL";
            Empty.SetActive(false);
        }
        else
        {
            timeData = SaveManager.Instance.GetTimeFromSave(slotIndex);
            // 格式化时间字符串
            string timeString = (timeData != null)
                ? $"第{timeData.year}学年{timeData.month}月{timeData.day}日"
                : "未知时间";

            slotText.text = $"存档 {slotIndex + 1}\n{timeString}</size>";
            Empty.SetActive(true);
        }
    }
}