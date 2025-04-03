using UnityEngine;
using UnityEngine.SceneManagement;

public class UICameraBinder : MonoBehaviour {
    [Header("绑定设置")]
    public string mainSceneName = "MainScene"; // 主场景名称
    public float planeDistance = 5f;          // Canvas 与摄像机的距离

    void Start() {
        // 方案 1：直接查找主摄像机（需确保主场景已加载）
        Camera mainCamera = Camera.main; // 要求主摄像机标签为 "MainCamera"
        
        // 方案 2：遍历场景查找摄像机（更保险）
        if (mainCamera == null) {
            mainCamera = FindObjectOfType<Camera>();
        }

        // 绑定到 Canvas
        Canvas canvas = GetComponent<Canvas>();
        if (mainCamera != null) {
            canvas.worldCamera = mainCamera;
            canvas.planeDistance = planeDistance; // 控制 UI 的渲染层级
        } else {
            Debug.LogError("未找到主摄像机！请确保主场景已加载。");
        }
    }
}