using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace SchoolD.Task
{
    public class TaskListItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI DescripText;//标题文本
        public void Setup(Task task)
        {
            DescripText.text = task.description;
        }

    }
}

