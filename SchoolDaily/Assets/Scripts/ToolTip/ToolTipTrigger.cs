using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ToolTipTrigger : MonoBehaviour//, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    [TextArea(3, 10)] // 添加这个特性使Inspector中显示多行文本区域
    public string content = "默认内容";

    [SerializeField]
    public string header = "默认标题";

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"触发提示 - 标题: {header}, 内容: {content}"); // 调试日志
        ToolTipSystem.Show(content, header);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTipSystem.Hide();
    }
}
