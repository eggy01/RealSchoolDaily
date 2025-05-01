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
        [Tooltip("留空将自动查找当前场景的灯光父对象")]
        public Transform manualLightParent;
        public float fadeDuration = 2f;

        [NonSerialized] public List<Light2D> lights = new List<Light2D>();
        [NonSerialized] public bool isOn;
    }

    [Header("灯光配置")]
    public List<LightGroup> lightGroups = new List<LightGroup>();

    [Header("持久场景灯光")]
    public List<Light2D> persistentLights = new List<Light2D>();

    [Header("设置")]
    public float checkInterval = 0.5f;

    private Scene currentActiveScene;
    private Coroutine checkCoroutine;

    private void Awake()
    {
        currentActiveScene = SceneManager.GetActiveScene();
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void Start()
    {
        RefreshSceneLights();
        StartLightCheck();
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        currentActiveScene = newScene;
        RefreshSceneLights();
    }

    public void RefreshSceneLights()
    {
        // 1. 清理持久场景灯光中的无效引用
        persistentLights = persistentLights.Where(light => light != null).ToList();

        // 2. 处理动态场景灯光
        foreach (var group in lightGroups)
        {
            group.lights.Clear();

            Transform lightParent = group.manualLightParent;

            // 自动查找逻辑
            if (lightParent == null && SceneLightManager.Instance != null)
            {
                lightParent = SceneLightManager.Instance.GetLightParentForScene(currentActiveScene.name);
            }

            if (lightParent != null && lightParent.gameObject.scene == currentActiveScene)
            {
                group.lights = lightParent.GetComponentsInChildren<Light2D>(true)
                    .Where(light => light != null)
                    .ToList();

                Debug.Log($"收集到 {currentActiveScene.name} 的 {group.groupName} 灯光: {group.lights.Count}个",
                    lightParent);
            }
        }
    }

    public void SetAllLights(bool turnOn)
    {
        // 控制持久场景灯光
        SetLightsImmediate(persistentLights, turnOn);

        // 控制当前场景灯光
        foreach (var group in lightGroups)
        {
            if (group.lights.Count > 0)
            {
                StartCoroutine(FadeLightGroup(group, turnOn));
            }
        }
    }

    private IEnumerator FadeLightGroup(LightGroup group, bool fadeIn)
    {
        group.lights = group.lights.Where(light => light != null).ToList();
        group.isOn = fadeIn;

        float end = fadeIn ? 0 : 1;
        float start = fadeIn ? 1 : 0;
        float timer = 0;

        // 初始化状态
        foreach (var light in group.lights)
        {
            if (light != null)
            {
                light.gameObject.SetActive(true);
                light.intensity = start;
            }
        }

        // 淡入淡出
        while (timer < group.fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / group.fadeDuration);

            foreach (var light in group.lights)
            {
                if (light != null)
                {
                    light.intensity = Mathf.Lerp(start, end, t);
                }
            }
            yield return null;
        }

        // 淡出时禁用灯光
        if (!fadeIn)
        {
            foreach (var light in group.lights)
            {
                if (light != null)
                {
                    light.gameObject.SetActive(false);
                }
            }
        }
    }

    private void SetLightsImmediate(List<Light2D> lights, bool on)
    {
        foreach (var light in lights.Where(light => light != null))
        {
            light.intensity = on ? 1 : 0;
            light.gameObject.SetActive(on);
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
            // 这里可以添加定期检查逻辑
        }
    }

    [ContextMenu("手动刷新灯光")]
    public void ManualRefreshLights()
    {
        RefreshSceneLights();
    }
}