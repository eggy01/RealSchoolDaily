using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaDialogueManager : MonoBehaviour
{
    public static AreaDialogueManager Instance;

    [System.Serializable]
    public class AreaDialogue
    {
        public string areaName;
        public TextAsset dialogueCSV;
    }

    public List<AreaDialogue> areaDialogues = new List<AreaDialogue>();
    private Dictionary<string, TextAsset> areaDialogueMap = new Dictionary<string, TextAsset>();

    private void Awake()
    {
        Instance = this;
        foreach (var item in areaDialogues)
        {
            areaDialogueMap[item.areaName] = item.dialogueCSV;
        }
    }

    public void TriggerAreaDialogue(string areaName, Collider2D other)
    {
        if (other.CompareTag("Player") && areaDialogueMap.TryGetValue(areaName, out TextAsset csv))
        {
            var dialogueList = DialogueCSVReader.Instance.LoadDialogueData(csv);
            EventHandler.CallStartNewDialogueEvent(dialogueList, areaName);
        }
    }
}
