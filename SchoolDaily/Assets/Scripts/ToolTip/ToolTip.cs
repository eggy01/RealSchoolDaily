using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToolTip : MonoBehaviour
{
    public TextMeshProUGUI headerField;
    public TextMeshProUGUI contentField;
    public LayoutElement layoutElement;
    public int characterWrapLimit = 40;

    private void Awake()
    {
        // 初始化时清空文本
        headerField.text = "";
        contentField.text = "";
    }

    public void SetText(string content, string header = "")
    {
        // 调试日志
        Debug.Log($"设置ToolTip文本 - 标题: {header}, 内容: {content}");

        // 设置标题
        if (string.IsNullOrEmpty(header))
        {
            headerField.gameObject.SetActive(false);
        }
        else
        {
            headerField.gameObject.SetActive(true);
            headerField.text = header;
        }

        // 设置内容
        contentField.text = content ?? ""; // 处理null情况

        // 根据文本长度决定是否换行
        bool shouldWrap = (headerField.text.Length > characterWrapLimit) ||
                         (contentField.text.Length > characterWrapLimit);
        layoutElement.enabled = shouldWrap;
    }

    private void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            Vector2 position = Input.mousePosition;
            transform.position = position;
        }
    }
}
