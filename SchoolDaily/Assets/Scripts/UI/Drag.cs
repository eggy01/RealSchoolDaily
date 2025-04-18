using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ForceScrollWheel : MonoBehaviour
{
    public float scrollSensitivity = 1f; // 滚轮灵敏度
    private ScrollRect scrollRect;

    void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
    }

    void Update()
    {
        // 检测鼠标滚轮输入
        float wheelInput = Input.GetAxis("Mouse ScrollWheel");
        
        if (wheelInput != 0 && scrollRect.vertical) // 如果是垂直滚动
        {
            // 直接调整滚动位置
            scrollRect.verticalNormalizedPosition += wheelInput * scrollSensitivity;
            scrollRect.verticalNormalizedPosition = Mathf.Clamp(scrollRect.verticalNormalizedPosition, 0f, 1f);
        }
        else if (wheelInput != 0 && scrollRect.horizontal) // 如果是水平滚动
        {
            scrollRect.horizontalNormalizedPosition += wheelInput * scrollSensitivity;
            scrollRect.horizontalNormalizedPosition = Mathf.Clamp(scrollRect.horizontalNormalizedPosition, 0f, 1f);
        }
    }
}