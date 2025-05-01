using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleObject : MonoBehaviour
{
    public Button button; // 引用按钮组件
    public GameObject buttonImage; // 引用按钮上的图片组件

    void Start()
    {
        // 初始时设置为未选中状态
        if (buttonImage != null)
        {
            buttonImage.SetActive(false);
        }
        else
        {
            Debug.LogError("buttonImage 未引用");
        }

        // 添加事件监听需要通过 EventTrigger
        EventTrigger eventTrigger = button.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = button.gameObject.AddComponent<EventTrigger>();
        }

        // 添加选中事件
        EventTrigger.Entry entrySelect = new EventTrigger.Entry();
        entrySelect.eventID = EventTriggerType.Select;
        entrySelect.callback.AddListener((data) => { OnButtonSelect(data); });
        eventTrigger.triggers.Add(entrySelect);

        // 添加取消选中事件
        EventTrigger.Entry entryDeselect = new EventTrigger.Entry();
        entryDeselect.eventID = EventTriggerType.Deselect;
        entryDeselect.callback.AddListener((data) => { OnButtonDeselect(data); });
        eventTrigger.triggers.Add(entryDeselect);
    }

    void OnButtonSelect(BaseEventData eventData)
    {
        // 按钮被选中时
        if (buttonImage != null)
        {
            buttonImage.SetActive(true);
        }
    }

    void OnButtonDeselect(BaseEventData eventData)
    {
        // 按钮未被选中时
        if (buttonImage != null)
        {
            buttonImage.SetActive(false);
        }
    }
}