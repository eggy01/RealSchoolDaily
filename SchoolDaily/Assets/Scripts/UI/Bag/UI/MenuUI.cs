using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
<<<<<<< Updated upstream
    public GameObject[] panels; // 存放所有界面
    public Button[] buttons;    // 存放所有按钮

    private int currentIndex = -1; // 当前显示的界面索引
    public int initialPanelIndex = 2; // 初始显示的界面索引

    void Start()
    {
        // 初始隐藏所有界面
=======
    public GameObject[] panels;
    public Button[] buttons;
    private int currentIndex = -1;

    private void Start()
    {
        InitializePanels();
        SetupButtonListeners();
    }

    private void InitializePanels()
    {
>>>>>>> Stashed changes
        foreach (GameObject panel in panels)
        {
            panel.SetActive(false);
        }
<<<<<<< Updated upstream

        // 显示初始界面
        if (initialPanelIndex >= 0 && initialPanelIndex < panels.Length)
        {
            panels[initialPanelIndex].SetActive(true);
            currentIndex = initialPanelIndex;

            // 将对应的按钮设置为选中状态
            if (buttons.Length > initialPanelIndex)
            {
                buttons[initialPanelIndex].Select();
            }
        }

        // 为每个按钮添加事件监听，使用局部变量避免闭包问题
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; // 创建局部变量
            buttons[i].onClick.AddListener(() => ShowPanel(index));
        }
    }

    void ShowPanel(int index)
    {
        // 检查索引有效性
        if (index < 0 || index >= panels.Length)
        {
            Debug.LogError($"无效的界面索引: {index}");
            return;
        }

        // 隐藏当前界面
=======
        ShowPanel(2);
    }

    private void SetupButtonListeners()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.AddListener(() => 
            {
                ShowPanel(index);
                InventoryUIHandler.Instance.CloseAllDetails();
            });
        }
    }

    private void ShowPanel(int index)
    {
        if (currentIndex == index) return;

>>>>>>> Stashed changes
        if (currentIndex != -1)
        {
            panels[currentIndex].SetActive(false);
        }

<<<<<<< Updated upstream
        // 显示新界面
=======
>>>>>>> Stashed changes
        panels[index].SetActive(true);
        currentIndex = index;
    }
}