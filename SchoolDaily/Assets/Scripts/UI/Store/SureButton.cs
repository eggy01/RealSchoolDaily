using UnityEngine;
using UnityEngine.UI;

public class SureButton : MonoBehaviour
{
    [Header("组件配置")]
    public Button targetButton;
    public GameObject SurePanel; // 需要关闭的提示框
    public GameObject Mask;

    private Button maskBtn;

    private void Update()
    {
        Mask.SetActive(true);

        // 绑定目标按钮点击事件
        targetButton.onClick.AddListener(ClosePanelAndMask);

        // 获取或添加遮罩按钮组件并绑定点击事件
        maskBtn = Mask.GetComponent<Button>() ?? Mask.AddComponent<Button>();
        maskBtn.transition = Selectable.Transition.None; // 禁用按钮过渡效果
        maskBtn.onClick.AddListener(ClosePanelAndMask);
    }

    private void ClosePanelAndMask()
    {
        SurePanel.SetActive(false);
        Mask.SetActive(false);
    }

    private void OnDestroy()
    {
        // 移除目标按钮点击事件
        targetButton.onClick.RemoveListener(ClosePanelAndMask);

        // 如果遮罩按钮存在，移除其点击事件
        if (maskBtn != null)
        {
            maskBtn.onClick.RemoveListener(ClosePanelAndMask);
        }
    }
}