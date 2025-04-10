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
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("目标场景：" + sceneToGo);
                EventHandler.CallTransitionEvent(sceneToGo, positionToGo);

            }
        }
    }


}
