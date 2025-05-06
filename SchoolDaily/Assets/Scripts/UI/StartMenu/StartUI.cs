using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartUI : MonoBehaviour
{
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI dateText;
    
    void Update()
    {
        DateTime currentTime = DateTime.Now;
        dateText.text = currentTime.ToString("yyyy-MM-dd");
        timeText.text = currentTime.ToString("HH:mm");
    }
}
