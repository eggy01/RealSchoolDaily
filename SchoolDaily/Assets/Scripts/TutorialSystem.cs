using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SchoolD.Dialogue;
using UnityEngine.UI;

public class TutorialSystem : MonoBehaviour
{
    public enum TutorialPhase
    {
        None,
        InventoryTutorial,
        ShoppingTutorial,
        CourseSelectionTutorial,
        Completed
    }

    public static TutorialSystem Instance;
    public TutorialPhase currentPhase = TutorialPhase.None;

    [Header("References")]
    public InventoryManager inventorySystem;
    public Button storageButton;
    public Button book;

    private bool waitingForBKey = false;
    private bool isBookClicked = false;

    private void Awake()
    {
        Instance = this;
    }

    public void StartInventoryTutorial(string s)
    {
        currentPhase = TutorialPhase.InventoryTutorial;

        if (s.Contains("点击"))
        {
            Debug.Log("开始背包教程 - 点击阶段");
            book.gameObject.SetActive(true);
            book.onClick.RemoveAllListeners();
            book.onClick.AddListener(OnBookClicked);
        }
        else if (s.Contains("按B"))
        {
            Debug.Log("开始背包教程 - 按B阶段");
            waitingForBKey = true;
        }
    }

    private void OnBookClicked()
    {
        Debug.Log("课本被点击");
        isBookClicked = true;

        storageButton.gameObject.SetActive(true);

        storageButton.onClick.RemoveAllListeners();
        storageButton.onClick.AddListener(OnStorageButtonClicked);
    }

    private void OnStorageButtonClicked()
    {
        Debug.Log("存储按钮被点击");
        PackageLocalData.Instance.AddItem("F001");

        InventoryUIHandler.Instance.ToggleInventory();

        CompleteTutorial();
    }

    private void Update()
    {
        if (waitingForBKey && Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("B键被按下");

            InventoryUIHandler.Instance.ToggleInventory();

            CompleteTutorial();
        }
    }

    public IEnumerator WaitForTutorialComplete()
    {
        while (currentPhase != TutorialPhase.Completed)
        {
            yield return null;
        }
    }

    private void CompleteTutorial()
    {
        book.gameObject.SetActive(false);
        storageButton.gameObject.SetActive(false);
        Debug.Log("完成背包教程");
        currentPhase = TutorialPhase.Completed;
        waitingForBKey = false;
        isBookClicked = false;
    }
}
