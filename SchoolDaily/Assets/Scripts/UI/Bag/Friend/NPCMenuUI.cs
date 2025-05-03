using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class NPCMenuUI : MonoBehaviour
{
    public static NPCMenuUI Instance;

    [Header("UI组件")]
    public Transform contentParent;       // NPC列表容器
    public GameObject npcItemPrefab;      // NPC项预制体
    public GameObject detailPanelPrefab;  // 详情面板预制体
    public Sprite defaultNPCIcon;         // 默认头像

    [Header("详情面板映射")]
    private Dictionary<GameObject, GameObject> npcDetailMap = new Dictionary<GameObject, GameObject>(); // 存储NPC项与对应详情面板的关系

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        NPCManager.onNPCDataChanged.AddListener(RefreshNPCList);
        RefreshNPCList();
    }

    private void OnDisable()
    {
        NPCManager.onNPCDataChanged.RemoveListener(RefreshNPCList);
    }

    // 刷新NPC列表
    public void RefreshNPCList()
    {
        // 清理所有详情面板
        foreach (var pair in npcDetailMap)
        {
            Destroy(pair.Value);
        }
        npcDetailMap.Clear();

        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        List<NPCData> allNPCs = NPCLoad.Instance.npcDatabase;

        foreach (NPCData npcData in allNPCs)
        {
            // 获取动态数据并过滤未遇见NPC
            NPCLocalItem localData = NPCManager.Instance.GetNPCData(npcData.NPCID);
            if (localData == null || !localData.IsMeet) continue;

            GameObject newItem = Instantiate(npcItemPrefab, contentParent);
            NPCItemUI itemUI = newItem.GetComponent<NPCItemUI>();

            itemUI.Setup(npcData, localData, defaultNPCIcon);

            Button btn = newItem.GetComponent<Button>();
            btn.onClick.AddListener(() => ToggleNPCDetail(newItem, npcData, localData));
        }
    }

    // 切换NPC详情显示/隐藏
    private void ToggleNPCDetail(GameObject npcItem, NPCData staticData, NPCLocalItem dynamicData)
    {
        NPCItemUI itemUI = npcItem.GetComponent<NPCItemUI>();
        // 检查是否已存在该NPC详情面板
        if (npcDetailMap.TryGetValue(npcItem, out GameObject existingPanel))
        {
            // 已存在则关闭
            CloseNPCDetailPanel(npcItem);
        }
        else
        {
            // 创建新面板
            CreateNPCDetailPanel(npcItem, staticData, dynamicData);
            itemUI.SetTagActive(true); //切换到Selected状态
        }
    }

    // 创建NPC详情面板
    private void CreateNPCDetailPanel(GameObject npcItem, NPCData staticData, NPCLocalItem dynamicData)
    {
        // 实例化面板并插入正确位置
        Transform contentTransform = contentParent;
        GameObject newPanel = Instantiate(detailPanelPrefab, contentTransform);
        newPanel.transform.SetSiblingIndex(npcItem.transform.GetSiblingIndex() + 1);

        NPCItemUI itemUI = npcItem.GetComponent<NPCItemUI>();
        NPCDetailUI detailUI = newPanel.GetComponent<NPCDetailUI>();
        if (itemUI && detailUI)
        {
            itemUI.tagImage.sprite = itemUI.selectTag;
            detailUI.Setup(staticData, dynamicData);
        }

        // 获取关闭按钮
        Button closeBtn = detailUI.transform.Find("close").GetComponent<Button>();
        closeBtn.onClick.AddListener(() => CloseNPCDetailPanel(npcItem));

        // 存储关系
        npcDetailMap.Add(npcItem, newPanel);
    }

    // 关闭NPC详情面板
    private void CloseNPCDetailPanel(GameObject targetNPCItem)
    {
        if (npcDetailMap.TryGetValue(targetNPCItem, out GameObject panel))
        {
            NPCItemUI itemUI = targetNPCItem.GetComponent<NPCItemUI>();
            itemUI.tagImage.sprite = itemUI.normlTag;
            Destroy(panel);
            npcDetailMap.Remove(targetNPCItem);
        }
    }
}