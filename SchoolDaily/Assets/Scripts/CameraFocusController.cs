using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using System;

public class CameraFocusController : MonoBehaviour
{
    [Header("相机设置")]
    public CinemachineVirtualCamera playerFollowCamera; // 跟随玩家的虚拟相机
    public CinemachineVirtualCamera bedFocusCamera;     // 聚焦床铺的虚拟相机
    public float focusDuration = 2f;                   // 聚焦持续时间

    [Header("床铺设置")]
    public Transform bedTransform;          // 床铺的Transform
    public Vector2 focusOffset = new Vector2(0, 0.5f); // 垂直偏移
    public float focusOrthoSize = 3f;       // 聚焦时的相机大小

    private float originalOrthoSize;

    private void Start()
    {
        // 初始状态：只启用跟随相机
        playerFollowCamera.Priority = 10;
        bedFocusCamera.Priority = 0;

        // 记录原始相机大小
        originalOrthoSize = playerFollowCamera.m_Lens.OrthographicSize;
    }

    public void FocusOnBed()
    {
        if (bedTransform == null)
        {
            Debug.LogError("床铺Transform未分配！");
            return;
        }

        // 设置床铺相机的位置
        SetupBedCamera();

        // 切换相机优先级
        playerFollowCamera.Priority = 0;
        bedFocusCamera.Priority = 10;

        // 指定时间后恢复
        Invoke(nameof(ReturnToPlayer), focusDuration);
    }

    private void SetupBedCamera()
    {
        // 设置相机位置（忽略Z轴）
        Vector3 targetPos = bedTransform.position + (Vector3)focusOffset;
        targetPos.z = bedFocusCamera.transform.position.z; // 保持原始Z轴

        bedFocusCamera.transform.position = targetPos;
        bedFocusCamera.m_Lens.OrthographicSize = focusOrthoSize;
    }

    private void ReturnToPlayer()
    {
        playerFollowCamera.Priority = 10;
        bedFocusCamera.Priority = 0;
    }
}
