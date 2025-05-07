using UnityEngine;
using UnityEngine.UI;

public class ToggleObject : MonoBehaviour
{
    public Image background; // 按钮的背景图片组件
    public GameObject buttonImage; // 引用按钮上的图片组件
    public Color normalColor;
    public Color selectedColor;


    public void SetSelected(bool isSelected)
    {
        if (buttonImage != null)
        {
            buttonImage.SetActive(isSelected);
            background.color = isSelected ? selectedColor : normalColor;
        }
    }
}