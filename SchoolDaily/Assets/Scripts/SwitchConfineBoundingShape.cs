using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class SwitchConfineBoundingShape : MonoBehaviour
{
    private void OnEnable()
    {
        EventHandler.AfterScenLoadEvent += SwitchBoundingShape;
    }
    private void OnDisable()
    {
        EventHandler.AfterScenLoadEvent -= SwitchBoundingShape;
    }
    private void SwitchBoundingShape()
    {
        GameObject boundsObject = GameObject.FindGameObjectWithTag("BoundsConfiner");
        if (boundsObject != null)
        {
            PolygonCollider2D polygonCollider2D = boundsObject.GetComponent<PolygonCollider2D>();

            if (polygonCollider2D != null)
            {
                CinemachineConfiner cinemachineConfiner = GetComponent<CinemachineConfiner>();
                if (cinemachineConfiner != null)
                {
                    cinemachineConfiner.m_BoundingShape2D = polygonCollider2D;
                    cinemachineConfiner.InvalidatePathCache();
                }
            }
        }
    }
}
