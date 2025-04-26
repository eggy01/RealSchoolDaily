using System.Collections;
using System.Collections.Generic;
using SchoolD.Dialogue;
using UnityEngine;

public class TimeTrigger : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        ConditionSystem.Check("时间.==.23:00");
    }
}
