using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class QAPair
{
    public string Q;
    public string A;
}

[System.Serializable]
public class DeveloperData
{
    public string developerName;
    public List<QAPair> qaList;
}

[System.Serializable]
public class DeveloperLogWrapper
{
    public List<DeveloperData> developers;
}

public class DeveloperLogUI : MonoBehaviour
{
    [Header("UI配置")]
    public TextAsset jsonFile;
    public GameObject nameButtonPrefab;
    public Transform buttonsParent;
    public GameObject qaPrefab;
    public Transform contentParent;
    public GameObject nullIndicatorPrefab;

    [Header("按钮状态图片")]
    public Sprite normalButtonSprite;    // 普通状态图片
    public Sprite selectedButtonSprite;  // 选中状态图片

    private DeveloperLogWrapper logData;
    private Dictionary<string, List<QAPair>> dataDictionary;
    private List<Button> developerButtons = new List<Button>(); // 保存所有开发者按钮
    private Button currentlySelectedButton; // 当前选中的按钮

    void Start()
    {
        ParseJsonData();
        InitializeDictionary();
        CreateDeveloperButtons();
    }

    void ParseJsonData()
    {
        logData = JsonUtility.FromJson<DeveloperLogWrapper>(jsonFile.text);
    }

    void InitializeDictionary()
    {
        dataDictionary = new Dictionary<string, List<QAPair>>();
        foreach (var dev in logData.developers)
        {
            dataDictionary.Add(dev.developerName, dev.qaList);
        }
    }

    void CreateDeveloperButtons()
    {
        foreach (var name in dataDictionary.Keys)
        {
            GameObject buttonObj = Instantiate(nameButtonPrefab, buttonsParent);
            buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = name;

            Button button = buttonObj.GetComponent<Button>();
            developerButtons.Add(button);

            // 设置初始状态图片
            button.GetComponent<Image>().sprite = normalButtonSprite;

            // 使用临时变量解决闭包问题
            string currentName = name;
            button.onClick.AddListener(() => 
            {
                OnDeveloperButtonClicked(button, currentName);
            });
        }
    }

    // 处理按钮点击事件
    void OnDeveloperButtonClicked(Button clickedButton, string developerName)
    {
        // 重置所有按钮状态
        foreach (Button btn in developerButtons)
        {
            btn.GetComponent<Image>().sprite = normalButtonSprite;
        }

        // 设置当前选中按钮状态
        clickedButton.GetComponent<Image>().sprite = selectedButtonSprite;
        currentlySelectedButton = clickedButton;

        // 更新QA内容
        ShowDeveloperQA(developerName);
    }

    void ShowDeveloperQA(string developerName)
    {
        ClearContent();

        if (dataDictionary.TryGetValue(developerName, out List<QAPair> qaList))
        {
            if (qaList == null || qaList.Count == 0)
            {
                Instantiate(nullIndicatorPrefab, contentParent);
            }
            else
            {
                foreach (var qa in qaList)
                {
                    CreateQAItem(qa.Q, qa.A);
                }
            }
        }
        else
        {
            Instantiate(nullIndicatorPrefab, contentParent);
        }
    }

    void CreateQAItem(string question, string answer)
    {
        GameObject qaItem = Instantiate(qaPrefab, contentParent);
        qaItem.GetComponent<QAItem>().Initialize(question, answer);
    }

    void ClearContent()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }
}