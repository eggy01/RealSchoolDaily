using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private ScrollRect parentScrollRect;

    private void Start()
    {
        parentScrollRect = GetComponentInParent<ScrollRect>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (parentScrollRect != null)
        {
            parentScrollRect.OnBeginDrag(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (parentScrollRect != null)
        {
            parentScrollRect.OnDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (parentScrollRect != null)
        {
            parentScrollRect.OnEndDrag(eventData);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 触发点击事件
        Debug.Log("Clicked: " + gameObject.name);
    }
}