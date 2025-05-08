using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightSystem : MonoBehaviour
{
    [Header("光照设置")]
    public Light2D globalLight;
    public Gradient lightColorGradient;
    [Range(0.1f, 5f)] public float transitionSpeed = 1f;

    [Header("季节参数")]
    public float summerDayLength = 16f;  // 夏季白天时长(小时)
    public float winterDayLength = 8f;   // 冬季白天时长(小时)
    public float summerMaxIntensity = 1f;
    public float winterMaxIntensity = 0.8f;
    public float baseMinIntensity = 0.1f;

    [Header("路灯设置")]
    public float lightActivationThreshold = 0.2f;
    public float streetLightTransitionTime = 2f;

    // 私有变量
    private float targetIntensity;
    private Color targetColor;
    private float seasonLerpFactor;
    private Coroutine lightTransitionCoroutine;
    private SceneLightController lightController;
    private bool streetLightsOn;

    private float currentTimeOfDay;

    private void Awake()
    {
        // 确保单例
        if (FindObjectsOfType<DayNightSystem>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        // 初始化路灯控制器
        lightController = FindObjectOfType<SceneLightController>();
        if (lightController == null)
        {
            GameObject controllerObj = new GameObject("SceneLightController");
            lightController = controllerObj.AddComponent<SceneLightController>();
            lightController.lightActivationThreshold = lightActivationThreshold;
        }
    }

    private void OnEnable()
    {
        // 注册时间事件
        TimeManager.Instance.OnHourChanged += OnHourChanged;
        EventHandler.AfterScenLoadEvent += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (TimeManager.Instance != null)
            TimeManager.Instance.OnHourChanged -= OnHourChanged;
        EventHandler.AfterScenLoadEvent -= OnSceneLoaded;
    }

    private void Start()
    {
        InitializeLightSettings();
        UpdateLightingImmediately(); // 立即更新一次光照
    }

    private void InitializeLightSettings()
    {
        // 更平滑的光照渐变设置
        lightColorGradient = new Gradient()
        {
            colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.05f, 0.05f, 0.2f), 0f),    // 午夜
                new GradientColorKey(new Color(0.2f, 0.2f, 0.4f), 0.23f),    // 黎明前
                new GradientColorKey(new Color(1f, 0.7f, 0.5f), 0.25f),     // 日出
                new GradientColorKey(new Color(1f, 0.95f, 0.9f), 0.3f),      // 白天
                new GradientColorKey(new Color(1f, 0.7f, 0.5f), 0.7f),      // 日落前
                new GradientColorKey(new Color(0.2f, 0.2f, 0.4f), 0.75f),   // 日落
                new GradientColorKey(new Color(0.05f, 0.05f, 0.2f), 0.77f)  // 夜晚
            },
            alphaKeys = new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        };
    }

    private void OnHourChanged(int hour)
    {
        // 每小时更新光照
        UpdateLightingSmoothly();
    }

    private void OnSceneLoaded()
    {
        // 场景加载后立即更新光照
        UpdateLightingImmediately();
    }

    public void UpdateTimeFromManager(int hour, int minute)
    {
        // 计算当前时间比例 (0-1)
        currentTimeOfDay = (hour + minute / 60f) / 24f;
        UpdateLightingImmediately();
    }

    private void UpdateLightingImmediately()
    {
        CalculateTargetLightValues();
        ApplyLightingImmediately();
        UpdateStreetLights();
    }

    private void UpdateLightingSmoothly()
    {
        CalculateTargetLightValues();

        if (lightTransitionCoroutine != null)
            StopCoroutine(lightTransitionCoroutine);

        lightTransitionCoroutine = StartCoroutine(SmoothLightTransition());
    }

    private void CalculateTargetLightValues()
    {
        // 从时间管理器获取季节信息
        Season currentSeason = TimeManager.Instance.GetSeason();
        seasonLerpFactor = CalculateSeasonLerpFactor(currentSeason, TimeManager.Instance.GetMonth());

        float currentDayLength = Mathf.Lerp(winterDayLength, summerDayLength, seasonLerpFactor);
        float currentMaxIntensity = Mathf.Lerp(winterMaxIntensity, summerMaxIntensity, seasonLerpFactor);

        // 计算当前时间比例 (0-1)
        float currentHour = TimeManager.Instance.GetHour() + TimeManager.Instance.GetMinute() / 60f;
        currentTimeOfDay = currentHour / 24f;

        // 计算昼夜时间点
        float dawnTime = (12f - currentDayLength / 2f) / 24f;
        float sunriseTime = dawnTime + 0.05f;
        float sunsetTime = (12f + currentDayLength / 2f - 0.05f) / 24f;
        float duskTime = (12f + currentDayLength / 2f) / 24f;

        // 计算目标光照强度
        if (currentTimeOfDay <= dawnTime || currentTimeOfDay >= duskTime)
        {
            targetIntensity = baseMinIntensity;
        }
        else if (currentTimeOfDay <= sunriseTime)
        {
            float t = (currentTimeOfDay - dawnTime) / (sunriseTime - dawnTime);
            targetIntensity = Mathf.Lerp(baseMinIntensity, currentMaxIntensity, t);
        }
        else if (currentTimeOfDay >= sunsetTime)
        {
            float t = (duskTime - currentTimeOfDay) / (duskTime - sunsetTime);
            targetIntensity = Mathf.Lerp(baseMinIntensity, currentMaxIntensity, t);
        }
        else
        {
            targetIntensity = currentMaxIntensity;
        }

        // 计算目标颜色
        targetColor = lightColorGradient.Evaluate(currentTimeOfDay);
    }

    private float CalculateSeasonLerpFactor(Season season, int month)
    {
        if (season == Season.夏天) return 1f;
        if (season == Season.冬天) return 0f;

        if (season == Season.春天)
            return (month - 3) / 3f; // 3月=0, 6月=1
        else // 秋天
            return 1 - (month - 9) / 3f; // 9月=1, 12月=0
    }

    private IEnumerator SmoothLightTransition()
    {
        float startIntensity = globalLight.intensity;
        Color startColor = globalLight.color;
        float elapsedTime = 0f;

        while (elapsedTime < transitionSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / transitionSpeed);

            globalLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
            globalLight.color = Color.Lerp(startColor, targetColor, t);

            // 检查路灯状态
            UpdateStreetLights();

            yield return null;
        }
    }

    private void ApplyLightingImmediately()
    {
        if (globalLight == null) return;

        globalLight.intensity = targetIntensity;
        globalLight.color = targetColor;
    }

    private void UpdateStreetLights()
    {
        if (lightController == null) return;

        bool shouldLightsBeOn = globalLight.intensity <= lightActivationThreshold;

        if (shouldLightsBeOn != streetLightsOn)
        {
            streetLightsOn = shouldLightsBeOn;
            lightController.SetAllLights(streetLightsOn);

            // 路灯渐亮渐暗效果
            if (streetLightTransitionTime > 0)
            {
                foreach (var group in lightController.lightGroups)
                {
                    StartCoroutine(FadeLightGroup(group, streetLightsOn, streetLightTransitionTime));
                }
            }
        }
    }

    private IEnumerator FadeLightGroup(SceneLightController.LightGroup group, bool turnOn, float duration)
    {
        float startIntensity = turnOn ? 0f : 1f;
        float targetIntensity = turnOn ? 1f : 0f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            foreach (var light in group.lights)
            {
                if (light != null)
                {
                    light.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
                }
            }

            yield return null;
        }
    }
}