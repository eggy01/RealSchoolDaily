using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolTipSystem : MonoBehaviour
{
    private static ToolTipSystem current;
    public ToolTip tooltip;
    public static GameObject SleepTip;//睡觉提示框

    private void Awake()
    {
        if (current != null && current != this)
        {
            Destroy(gameObject);
            return;
        }

        current = this;
        current.tooltip.gameObject.SetActive(false);

        // 调试：检查tooltip是否正常引用
        if (tooltip == null)
        {
            Debug.LogError("ToolTip引用丢失！", this);
        }
    }

    public static void Show(string content, string header = "")
    {
        if (current.tooltip == null)
        {
            Debug.LogError("ToolTip实例不存在！");
            return;
        }

        Debug.Log($"显示ToolTip - 标题: {header}, 内容: {content}");
        current.tooltip.SetText(content, header);
        current.tooltip.gameObject.SetActive(true);
    }

    public static void Hide()
    {
        if (current.tooltip != null)
        {
            current.tooltip.gameObject.SetActive(false);
        }
    }

}
