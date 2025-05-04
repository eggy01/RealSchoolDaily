using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    public GameObject[] panels;
    public Button[] menuButtons; // 专门存放需要管理的5个菜单按钮
    private int currentIndex = -1;

    private void Start()
    {
        InitializePanels();
        SetupButtonListeners();
    }

    private void InitializePanels()
    {
        foreach (GameObject panel in panels)
        {
            panel.SetActive(false);
        }
        ShowPanel(2);
    }

    private void SetupButtonListeners()
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            int index = i;
            menuButtons[i].onClick.AddListener(() =>
            {
                ShowPanel(index);
                UpdateButtonStates(index);
                BagUI.Instance.CloseAllDetails();
            });
        }
    }

    private void ShowPanel(int index)
    {
        if (currentIndex == index) return;

        if (currentIndex != -1)
        {
            panels[currentIndex].SetActive(false);
        }

        panels[index].SetActive(true);
        currentIndex = index;
    }

    // 新增方法：更新所有按钮的选中状态
    private void UpdateButtonStates(int selectedIndex)
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            var toggle = menuButtons[i].GetComponent<ToggleObject>();
            if (toggle != null)
            {
                toggle.SetSelected(i == selectedIndex);
            }
        }
    }
}