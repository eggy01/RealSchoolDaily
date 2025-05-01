using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
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
        foreach (GameObject panel in panels)
        {
            panel.SetActive(false);
        }
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

        if (currentIndex != -1)
        {
            panels[currentIndex].SetActive(false);
        }

        panels[index].SetActive(true);
        currentIndex = index;
    }
}