using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaTrigger : MonoBehaviour
{
    [SerializeField] private string areaName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        AreaDialogueManager.Instance.TriggerAreaDialogue(areaName, other);
    }
}
