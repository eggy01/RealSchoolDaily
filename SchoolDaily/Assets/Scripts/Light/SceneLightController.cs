using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class SceneLightController : MonoBehaviour
{
    [System.Serializable]
    public class LightGroup
    {
        public string groupName;
        public float fadeDuration = 1f;

        [NonSerialized] public List<Light2D> lights = new List<Light2D>();
        [NonSerialized] public bool isOn;
    }

    [Header("灯光配置")]
    public List<LightGroup> lightGroups = new List<LightGroup>();

    [Header("设置")]
    public float checkInterval = 0.3f;
    public float activationRadius = 20f;
    public float lightActivationThreshold = 0.2f; // 光照强度阈值，低于这个值才激活灯光

    private Dictionary<Light2D, bool> lightPool = new Dictionary<Light2D, bool>();
    private List<Light2D> activeLights = new List<Light2D>();
    private Transform playerTransform;
    private Coroutine checkCoroutine;
    private Light2D globalLight; // 用于检测全局光照强度

    private void Awake()
    {
        // 单例模式
        if (FindObjectsOfType<SceneLightController>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void Start()
    {
        InitializeSystem();
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void InitializeSystem()
    {
        FindPlayer();
        FindGlobalLight();
        RefreshSceneLights();
        InitializeLightPool();
        StartLightCheck();
    }

    private void FindPlayer()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogWarning("未找到玩家对象，将使用默认位置");
            playerTransform = transform; // 使用控制器自身作为回退位置
        }
    }

    private void FindGlobalLight()
    {
        // 查找场景中的全局光照（通常是一个没有形状的Light2D）
        globalLight = FindObjectOfType<Light2D>();
        if (globalLight != null && globalLight.lightType != Light2D.LightType.Global)
        {
            // 如果不是全局光，继续查找
            var allLights = FindObjectsOfType<Light2D>();
            globalLight = allLights.FirstOrDefault(light => light.lightType == Light2D.LightType.Global);
        }

        if (globalLight == null)
        {
            Debug.LogWarning("未找到全局光照，将默认视为夜晚");
        }
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        // 延迟处理以确保SceneLightAnchor已完成注册
        StartCoroutine(DelayedSceneChange());
    }

    private IEnumerator DelayedSceneChange()
    {
        yield return null; // 等待一帧
        InitializeSystem();
    }

    private void RefreshSceneLights()
    {
        // 归还所有当前激活的灯光
        ReturnAllLightsToPool();

        // 收集新场景的灯光
        foreach (var group in lightGroups)
        {
            group.lights.Clear();

            // 通过SceneLightManager获取灯光父对象
            Transform lightParent = SceneLightManager.Instance?.GetLightParentForScene(SceneManager.GetActiveScene().name);

            if (lightParent != null)
            {
                group.lights = lightParent.GetComponentsInChildren<Light2D>(true)
                    .Where(light => light != null && light.lightType != Light2D.LightType.Global) // 排除全局光
                    .ToList();

                Debug.Log($"收集到 {SceneManager.GetActiveScene().name} 的 {group.groupName} 灯光: {group.lights.Count}个");
            }
        }
    }

    private void InitializeLightPool()
    {
        lightPool.Clear();

        foreach (var group in lightGroups)
        {
            foreach (var light in group.lights)
            {
                if (!lightPool.ContainsKey(light))
                {
                    lightPool.Add(light, false);
                    light.gameObject.SetActive(false);
                }
            }
        }
    }

    private Light2D GetPooledLight(Vector2 position)
    {
        var availableLight = lightPool.FirstOrDefault(x => !x.Value).Key;
        if (availableLight != null)
        {
            lightPool[availableLight] = true;
            availableLight.transform.position = position;
            availableLight.gameObject.SetActive(true);
            return availableLight;
        }
        return null;
    }

    private void ReturnLightToPool(Light2D light)
    {
        if (light != null && lightPool.ContainsKey(light))
        {
            lightPool[light] = false;
            light.gameObject.SetActive(false);
        }
    }

    private void ReturnAllLightsToPool()
    {
        foreach (var light in activeLights)
        {
            ReturnLightToPool(light);
        }
        activeLights.Clear();
    }

    // 外部调用这个方法根据光照强度控制灯光
    public void SetAllLights(bool shouldLightsBeOn)
    {
        if (shouldLightsBeOn)
        {
            UpdateActiveLightsAroundPlayer();
        }
        else
        {
            ReturnAllLightsToPool();
        }
    }

    // 检查当前是否是夜晚（光照强度≤阈值）
    public bool IsNightTime()
    {
        // 如果没有找到全局光，默认视为夜晚
        if (globalLight == null) return true;

        // 检查全局光照强度是否低于阈值
        return globalLight.intensity <= lightActivationThreshold;
    }

    private void StartLightCheck()
    {
        if (checkCoroutine != null)
        {
            StopCoroutine(checkCoroutine);
        }
        checkCoroutine = StartCoroutine(LightCheckRoutine());
    }

    private IEnumerator LightCheckRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);

            // 只有夜晚时才检查玩家附近的灯光
            if (IsNightTime())
            {
                UpdateActiveLightsAroundPlayer();
            }
            else
            {
                ReturnAllLightsToPool();
            }
        }
    }

    private void UpdateActiveLightsAroundPlayer()
    {
        // 如果不在夜晚时间，直接返回所有灯光
        if (!IsNightTime())
        {
            ReturnAllLightsToPool();
            return;
        }

        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform == null) return;
        }

        Vector2 playerPos = playerTransform.position;
        List<Light2D> neededLights = new List<Light2D>();

        // 确定需要哪些灯光（仅在夜晚且玩家靠近时）
        foreach (var group in lightGroups)
        {
            neededLights.AddRange(group.lights
                .Where(light => Vector2.Distance(playerPos, light.transform.position) <= activationRadius));
        }

        // 归还不再需要的灯光
        List<Light2D> toRemove = activeLights.Where(light => !neededLights.Contains(light)).ToList();
        foreach (var light in toRemove)
        {
            ReturnLightToPool(light);
        }
        activeLights.RemoveAll(light => toRemove.Contains(light));

        // 激活新需要的灯光
        foreach (var neededLight in neededLights.Where(light => !activeLights.Contains(light)))
        {
            var light = GetPooledLight(neededLight.transform.position);
            if (light != null)
            {
                activeLights.Add(light);
            }
        }
    }

    [ContextMenu("手动刷新灯光")]
    public void ManualRefreshLights()
    {
        RefreshSceneLights();
        InitializeLightPool();
    }

    private void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, activationRadius);
        }
    }
}