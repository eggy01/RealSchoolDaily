using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SchoolD.Transition
{
    public class Teleport : MonoBehaviour
    {
        public String sceneToGo;
        public Vector3 positionToGo;
        string conntent = "";
        string header = "";
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("目标场景：" + sceneToGo);
                Debug.Log("目标位置：" + positionToGo);

                if (TimeManager.Instance.GetHour() >= 23)
                {
                    conntent = WeatherManager.Instance.IsOuterScene(sceneToGo) ? "太晚了，还是不要出门了" : "已经锁门了";
                    ToolTipSystem.Show(conntent, header);
                }
                else if (TimeManager.Instance.GetHour() <= 5)
                {
                    conntent = "太早了，还没有开门";
                    ToolTipSystem.Show(conntent, header);
                }
                else EventHandler.CallTransitionEvent(sceneToGo, positionToGo);

            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            ToolTipSystem.Hide();
        }
    }
}
