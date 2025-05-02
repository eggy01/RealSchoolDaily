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
        TimeManager.Instance.OnHourChanged += OnHourChanged;
        // UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnDisable()
    {
        //EventHandler.OnDayChangedEvent -= OnHourChanged;
        //TimeManager.OnHourChanged -= OnHourChanged;
        TimeManager.Instance.OnHourChanged -= OnHourChanged;
        //UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void Start()
    {
        //CheckCurrentScene();
        // 强制重置曲线（仅调试用）
        lightIntensityCurve = new AnimationCurve(
            new Keyframe(0f, 0.1f),    // 午夜
            new Keyframe(0.2f, 0.8f),  // 黎明开始
            new Keyframe(0.3f, 1f),    // 早晨（覆盖7:00）
            new Keyframe(0.7f, 1f),    // 傍晚
            new Keyframe(0.8f, 0.1f)   // 深夜
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

    private void UpdateTimeOfDay()
    {
        // 计算当前时间在一天中的比例(0-1)
        float hour = timeManager.GetHour();

        float minute = timeManager.GetMinute();

        currentTimeOfDay = (hour + minute / 60f) / 24f;
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
            // 非室外场景直接设为固定光照
            globalLight.intensity = 1f;
            globalLight.color = Color.white;
            return;
        }

        UpdateSeasonalParameters();


        // 计算当前季节的昼夜参数
        float currentDayLength = Mathf.Lerp(winterDayLength, summerDayLength, seasonLerpFactor);
        float currentMaxIntensity = Mathf.Lerp(winterMaxIntensity, summerMaxIntensity, seasonLerpFactor);

        // 调整天气系统的基础光照强度
        AdjustWeatherLightIntensity(currentMaxIntensity);

        // 计算日夜周期
        float dawnTime = (12f - currentDayLength / 2f) / 24f;
        float duskTime = (12f + currentDayLength / 2f) / 24f;


        // 计算光照强度
        float intensity;
        if (currentTimeOfDay < dawnTime || currentTimeOfDay > duskTime)
        {
            // 夜晚
            intensity = baseMinIntensity;
        }
        else if (currentTimeOfDay < dawnTime + 0.05f)
        {
            // 黎明过渡
            float t = (currentTimeOfDay - dawnTime) / 0.05f;
            intensity = Mathf.Lerp(baseMinIntensity, currentMaxIntensity, t);
        }
        else if (currentTimeOfDay > duskTime - 0.05f)
        {
            // 黄昏过渡
            float t = (duskTime - currentTimeOfDay) / 0.05f;
            intensity = Mathf.Lerp(baseMinIntensity, currentMaxIntensity, t);
        }
        else
        {
            // 白天
            intensity = currentMaxIntensity;
        }

        // 应用光照设置
        globalLight.color = lightColorGradient.Evaluate(currentTimeOfDay);

        float curveValue = lightIntensityCurve.Evaluate(currentTimeOfDay);
        //Debug.Log($"强度计算: {intensity:F2} * {curveValue:F2}");
        if (curveValue <= 0.01f) // 如果曲线异常
        {
            curveValue = 1f;    // 强制白天亮度
            Debug.LogWarning("光照曲线返回异常值，已强制修复");
        }

        globalLight.intensity = Mathf.Clamp(intensity * curveValue, 0.03f, 1f);
        //Debug.Log("当前灯光强度：" + globalLight.intensity);

        // 新增：根据光照强度控制路灯
        // 控制所有灯光（光照≤0.2时开启）
        bool shouldLightsBeOn = globalLight.intensity <= lightActivationThreshold;
        lightController.SetAllLights(shouldLightsBeOn);

        Debug.Log($"全局光照: {globalLight.intensity:F2} | " +
                 $"灯光状态: {(shouldLightsBeOn ? "开启" : "关闭")}");
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
