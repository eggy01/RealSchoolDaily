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

    private Dictionary<Light2D, bool> lightPool = new Dictionary<Light2D, bool>();
    private List<Light2D> activeLights = new List<Light2D>();
    private Transform playerTransform;
    private Coroutine checkCoroutine;

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
                    .Where(light => light != null)
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

    public void SetAllLights(bool turnOn)
    {
        if (turnOn)
        {
            UpdateActiveLightsAroundPlayer();
        }
        else
        {
            ReturnAllLightsToPool();
        }
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
            UpdateActiveLightsAroundPlayer();
        }
    }

    private void UpdateActiveLightsAroundPlayer()
    {
        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform == null) return;
        }

        Vector2 playerPos = playerTransform.position;
        List<Light2D> neededLights = new List<Light2D>();

        // 确定需要哪些灯光
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