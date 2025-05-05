using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class DayNightSystem : MonoBehaviour
{

    [Header("光照设置")]
    public Light2D globalLight; // 全局2D光源
    public Gradient lightColorGradient; // 根据时间变化的颜色渐变
    public AnimationCurve lightIntensityCurve; // 根据时间变化的强度曲线

    [Header("季节参数")]
    public float summerDayLength = 16f; // 夏季白天时长(小时)
    public float winterDayLength = 8f;  // 冬季白天时长(小时)
    [Range(0f, 1f)] public float currentTimeOfDay; // 当前一天中的时间比例(0-1)

    [Header("光照强度")]
    public float summerMaxIntensity = 1f;
    public float winterMaxIntensity = 0.8f;
    public float baseMinIntensity = 0.1f;

    [Header("路灯设置")]

    public float lightActivationThreshold = 0.2f; // 光照强度阈值

    private TimeManager timeManager;
    private WeatherManager weatherManager;
    private SceneLightController lightController;

    private float seasonLerpFactor; // 季节过渡因子(0=冬季,1=夏季)

    private void Awake()
    {
        timeManager = FindObjectOfType<TimeManager>();
        weatherManager = FindObjectOfType<WeatherManager>();

        lightController = FindObjectOfType<SceneLightController>();
        if (lightController == null)
        {
            GameObject controllerObj = new GameObject("SceneLightController");
            lightController = controllerObj.AddComponent<SceneLightController>();
        }
    }

    private void OnEnable()
    {
        //EventHandler.OnDayChangedEvent += OnHourChanged;
        //TimeManager.Instance.OnHourChanged += OnHourChanged;
        EventHandler.TenMinuteChanged += ChangedLight;
        EventHandler.AfterScenLoadEvent += ChangedLight;
        // UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnDisable()
    {
        //EventHandler.OnDayChangedEvent -= OnHourChanged;
        //TimeManager.OnHourChanged -= OnHourChanged;
        //TimeManager.Instance.OnHourChanged -= OnHourChanged;
        EventHandler.TenMinuteChanged -= ChangedLight;
        //UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnSceneChanged;
        EventHandler.AfterScenLoadEvent -= ChangedLight;
    }

    private void Start()
    {
        //CheckCurrentScene();
        // 强制重置曲线（仅调试用）
        lightIntensityCurve = new AnimationCurve(
    new Keyframe(0f, 0.1f),    // 午夜
    new Keyframe(0.23f, 0.1f),  // 黎明前
    new Keyframe(0.25f, 0.8f),  // 日出
    new Keyframe(0.3f, 1f),    // 完全日出
    new Keyframe(0.7f, 1f),    // 日落前
    new Keyframe(0.75f, 0.8f), // 日落
    new Keyframe(0.77f, 0.1f)  // 完全夜晚
);
        UpdateTimeOfDay(); // 添加这行初始化时间
        UpdateSeasonalParameters();
        UpdateLighting();

    }
    private void OnHourChanged(int hour)
    {
        UpdateTimeOfDay();
        UpdateLighting();
    }
    private void ChangedLight()
    {
        UpdateTimeOfDay();
        UpdateLighting();
    }

    private void UpdateTimeOfDay()
    {
        // 计算当前时间在一天中的比例(0-1)
        float hour = timeManager.GetHour();

        float minute = timeManager.GetMinute();

        currentTimeOfDay = (hour + minute / 60f) / 24f;

        // 添加调试输出
        //Debug.Log($"当前游戏时间: {hour}:{minute} => currentTimeOfDay: {currentTimeOfDay}");
    }

    private void UpdateSeasonalParameters()
    {
        Season currentSeason = timeManager.GetSeason();
        //Debug.Log("当前季节" + currentSeason);

        // 计算季节过渡因子(0=冬季,1=夏季)
        if (currentSeason == Season.夏天)
        {
            seasonLerpFactor = 1f;
        }
        else if (currentSeason == Season.冬天)
        {
            seasonLerpFactor = 0f;
        }
        else
        {
            // 春秋季节作为过渡
            int month = timeManager.GetMonth();
            if (currentSeason == Season.春天)
            {
                seasonLerpFactor = (month - 3) / 3f; // 3月=0, 6月=1
            }
            else // 秋天
            {
                seasonLerpFactor = 1 - (month - 9) / 3f; // 9月=1, 12月=0
            }
        }
        Debug.Log("季节因子" + seasonLerpFactor);
    }

    private void UpdateLighting()
    {
        if (globalLight == null) return;

        if (!WeatherManager.isOutdoorScene)
        {
            globalLight.intensity = 1f;
            globalLight.color = Color.white;
            return;
        }

        UpdateSeasonalParameters();

        // 1. 计算季节参数（严格限制范围）
        float currentDayLength = Mathf.Clamp(
            Mathf.Lerp(winterDayLength, summerDayLength, seasonLerpFactor),
            0.1f, 24f
        );
        float currentMaxIntensity = Mathf.Clamp(
            Mathf.Lerp(winterMaxIntensity, summerMaxIntensity, seasonLerpFactor),
            baseMinIntensity, 1f
        );

        // 2. 计算时间分段（防止除零和越界）
        float dawnTime = Mathf.Clamp((12f - currentDayLength / 2f) / 24f, 0f, 0.5f);
        float sunriseTime = Mathf.Clamp(dawnTime + 0.05f, dawnTime + 0.01f, 0.5f); // 确保 sunriseTime > dawnTime
        float sunsetTime = Mathf.Clamp((12f + currentDayLength / 2f - 0.05f) / 24f, 0.5f, 1f);
        float duskTime = Mathf.Clamp((12f + currentDayLength / 2f) / 24f, sunsetTime + 0.01f, 1f); // 确保 duskTime > sunsetTime

        // 3. 计算光照强度（严格限制插值参数）
        float intensity;
        if (currentTimeOfDay <= dawnTime || currentTimeOfDay >= duskTime)
        {
            intensity = baseMinIntensity; // 夜晚
        }
        else if (currentTimeOfDay <= sunriseTime)
        {
            float t = Mathf.Clamp01((currentTimeOfDay - dawnTime) / Mathf.Max(0.01f, sunriseTime - dawnTime));
            intensity = Mathf.Lerp(baseMinIntensity, currentMaxIntensity, t); // 黎明渐变
        }
        else if (currentTimeOfDay >= sunsetTime)
        {
            float t = Mathf.Clamp01((duskTime - currentTimeOfDay) / Mathf.Max(0.01f, duskTime - sunsetTime));
            intensity = Mathf.Lerp(baseMinIntensity, currentMaxIntensity, t); // 黄昏渐变
        }
        else
        {
            intensity = currentMaxIntensity; // 白天
        }

        // 4. 最终强度限制（双重保险）
        float finalIntensity = Mathf.Clamp(intensity, baseMinIntensity, currentMaxIntensity);
        globalLight.intensity = finalIntensity;
        globalLight.color = lightColorGradient.Evaluate(currentTimeOfDay);

        // 调试输出
        Debug.Log($"时间: {currentTimeOfDay:F3} | 分段: [{dawnTime:F3},{sunriseTime:F3}]→[{sunsetTime:F3},{duskTime:F3}]");
        Debug.Log($"强度: {intensity:F3} → 最终: {finalIntensity:F3} (最大允许: {currentMaxIntensity:F2})");

        // 控制路灯
        lightController.SetAllLights(finalIntensity <= lightActivationThreshold);
    }

    private void AdjustWeatherLightIntensity(float seasonMaxIntensity)
    {
        // 根据季节调整天气系统的基准光照强度
        if (weatherManager != null)
        {
            weatherManager.sunnyLightColor *= seasonMaxIntensity;
            weatherManager.rainyLightColor *= seasonMaxIntensity;
            weatherManager.snowyLightColor *= seasonMaxIntensity;
            //weatherManager.windyLightColor *= seasonMaxIntensity;
            weatherManager.cloudyLightColor *= seasonMaxIntensity;
        }
    }

}
