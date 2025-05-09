using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class SaveSlotUI : MonoBehaviour
{
    [SerializeField] private GameObject slotPanel;
    [SerializeField] private Transform slotsParent;
    [SerializeField] private GameObject slotPrefab;
    private List<SaveSlotButton> slotButtons = new List<SaveSlotButton>();
    private bool isForNewGame;

    private bool isNewGame;

    private void Start()
    {
        InitializeSlots();
    }

    private void InitializeSlots()
    {
        for (int i = 0; i < SaveManager.SaveSlotCount; i++)
        {
            var slotObj = Instantiate(slotPrefab, slotsParent);
            var slotButton = slotObj.GetComponent<SaveSlotButton>();
            slotButtons.Add(slotButton);

            int index = i;
            slotButton.GetComponent<Button>().onClick.AddListener(() => OnSlotSelected(index));
        }
    }

    // 新游戏入口方法
    public void HandleNewGame()
    {
        int emptySlot = SaveManager.Instance.FindFirstEmptySlot();
        if (emptySlot != -1)
        {
            isNewGame = true;
            StartCoroutine(StartNewGameRoutine(emptySlot));
        }
        else
        {
            ShowSlotSelection(true);
        }
    }

    // 加载游戏入口方法
    public void HandleLoadGame()
    {
        ShowSlotSelection(false);
    }

    private void ShowSlotSelection(bool forNewGame)
    {
        isForNewGame = forNewGame;
        slotPanel.SetActive(true);
        RefreshSlots();
    }

    private void RefreshSlots()
    {
        for (int i = 0; i < slotButtons.Count; i++)
        {
            bool isEmpty = SaveManager.Instance.IsSlotEmpty(i);
            slotButtons[i].SetSlotInfo(i, isEmpty);

            // 根据模式设置按钮交互状态
            Button btn = slotButtons[i].GetComponent<Button>();
            btn.interactable = isForNewGame || !isEmpty;
        }
    }

    private void OnSlotSelected(int slot)
    {
        if (!isForNewGame && SaveManager.Instance.IsSlotEmpty(slot))
            return;

        SaveManager.Instance.SetCurrentSlot(slot);

        if (isForNewGame)
        {
            isNewGame = true;
            StartCoroutine(StartNewGameRoutine(slot));
        }
        else
        {
            SaveManager.Instance.LoadGame(slot);
        }
    }

    private IEnumerator StartNewGameRoutine(int slot)
    {
        SaveManager.Instance.currentSlot = slot;
        // // 检测是否是新游戏
        // 将结果存储到 PlayerPrefs（供 PersistScene 读取）
        // 检查是否是新游戏（存档槽位为空）
        PlayerPrefs.SetInt("IsNewGame_" + slot, isNewGame ? 1 : 0);
        PlayerPrefs.Save();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("PersistScene");
        while (!asyncLoad.isDone) yield return null;
        SaveManager.Instance.NewGame(slot);

    }
}