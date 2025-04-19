using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;//标题文本
    [SerializeField] private Image statusIcon;//状态图标
    [SerializeField] private GameObject headerTemplate;//分类

    public void Setup(Task task, Sprite icon)
    {
        titleText.text = task.title;
        statusIcon.sprite = icon;
        headerTemplate.SetActive(false);
    }

    public void SetAsHeader(string headerText)
    {
        titleText.text = headerText;
        statusIcon.gameObject.SetActive(false);
        headerTemplate.SetActive(true);

        // 禁用按钮交互
        GetComponent<Button>().interactable = false;
    }
}
