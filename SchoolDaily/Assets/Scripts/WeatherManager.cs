using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WeatherManager : MonoBehaviour
{
    public enum WeatherType
    {
        Sunny,
        Rainy,
        Snowy,
        Windy,
        Cloudy
    }

    [Header("场景设置")]
    public List<string> outdoorScenes = new List<string>
    {
        "A Scene",
        "B Scene",
        "Teach Scene",
        "Life Scene"
    };

    [Header("光照设置")]
    public Light sunLight;
    public Color sunnyLightColor = new Color(1f, 1f, 0.95f);
    public Color rainyLightColor = new Color(0.7f, 0.7f, 0.8f);
    public Color snowyLightColor = new Color(0.9f, 0.9f, 1f);
    public Color windyLightColor = new Color(0.85f, 0.85f, 0.75f);
    public Color cloudyLightColor = new Color(0.8f, 0.8f, 0.8f);

    [Header("天气参数")]
    public float minWeatherDuration = 1f;
    public float maxWeatherDuration = 3f;
    private float currentWeatherDuration;
    private int daysPassedWithCurrentWeather;
    public float transitionDuration = 5f;

    [Header("粒子系统")]
    public ParticleSystem rainParticleSystem;
    public ParticleSystem snowParticleSystem;
    public ParticleSystem windParticleSystem;
    public ParticleSystem cloudParticleSystem;

    private WeatherType currentWeather;
    private TimeManager timeManager;
    private bool isOutdoorScene;

    private void Awake()
    {
        timeManager = FindObjectOfType<TimeManager>();
        CheckCurrentScene();
    }

    private void Start()
    {
        currentWeatherDuration = Random.Range(minWeatherDuration, maxWeatherDuration);
        daysPassedWithCurrentWeather = 0;
        ChangeRandomWeather();
    }

    private void OnEnable()
    {
        EventHandler.OnDayChangedEvent += OnDayChanged;
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnDisable()
    {
        EventHandler.OnDayChangedEvent -= OnDayChanged;
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        CheckCurrentScene();
        UpdateWeatherEffects();
    }

    private void CheckCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        isOutdoorScene = outdoorScenes.Contains(currentScene);
        Debug.Log($"当前场景: {currentScene}, 是室外场景: {isOutdoorScene}");
    }

    private void OnDayChanged()
    {
        if (!timeManager.gameClockPause)
        {
            daysPassedWithCurrentWeather++;

            if (daysPassedWithCurrentWeather >= currentWeatherDuration)
            {
                ChangeRandomWeather();
                daysPassedWithCurrentWeather = 0;
                currentWeatherDuration = Random.Range(minWeatherDuration, maxWeatherDuration);
            }
        }
    }

    private void ChangeRandomWeather()
    {
        WeatherType newWeather = GetSeasonalWeather();
        Debug.Log("天气：" + newWeather);
        SetWeather(newWeather);
    }

    private void UpdateWeatherEffects()
    {
        // 根据当前是否在室外场景重新激活/禁用天气效果
        SetWeather(currentWeather);
    }

    private WeatherType GetSeasonalWeather()
    {
        float rand = Random.value;

        Season season = timeManager.GetSeason();//获取当前季节
        Debug.Log("当前季节：" + season);
        int month = timeManager.GetMonth();//获取当前月份
        Debug.Log("当前月份：" + month);
        bool isInTerm = timeManager.GetisInTerm();

        switch (season)
        {
            case Season.春天: // 3-5月
                if (month == 3) // 早春
                {
                    if (rand < 0.5f) return WeatherType.Rainy;
                    if (rand < 0.7f) return WeatherType.Cloudy;
                    return WeatherType.Sunny;
                }
                else // 晚春
                {
                    if (rand < 0.4f) return WeatherType.Rainy;
                    if (rand < 0.6f) return WeatherType.Sunny;
                    return WeatherType.Cloudy;
                }

            case Season.夏天: // 6-8月
                if (month == 6) // 初夏
                {
                    if (rand < 0.7f) return WeatherType.Sunny;
                    if (rand < 0.85f) return WeatherType.Cloudy;
                    return WeatherType.Rainy;
                }
                else // 盛夏
                {
                    if (rand < 0.8f) return WeatherType.Sunny;
                    if (rand < 0.9f) return WeatherType.Cloudy;
                    return WeatherType.Rainy;
                }

            case Season.秋天: // 9-11月
                if (month == 9) // 初秋
                {
                    if (rand < 0.5f) return WeatherType.Windy;
                    if (rand < 0.7f) return WeatherType.Cloudy;
                    return WeatherType.Sunny;
                }
                else // 深秋
                {
                    if (rand < 0.6f) return WeatherType.Windy;
                    if (rand < 0.8f) return WeatherType.Cloudy;
                    return WeatherType.Rainy;
                }

            case Season.冬天: // 12-2月
                if (month == 12) // 初冬
                {
                    if (rand < 0.6f) return WeatherType.Snowy;
                    if (rand < 0.8f) return WeatherType.Cloudy;
                    return WeatherType.Windy;
                }
                else // 深冬
                {
                    if (rand < 0.7f) return WeatherType.Snowy;
                    if (rand < 0.85f) return WeatherType.Cloudy;
                    return WeatherType.Windy;
                }

            default:
                return WeatherType.Sunny;
        }
    }

    private void SetWeather(WeatherType weather)
    {
        currentWeather = weather;

        // 始终更新光照（室内外都需要）
        UpdateLighting(weather);

        // 只在室外场景激活粒子效果
        if (isOutdoorScene)
        {
            UpdateParticleEffects(weather);
        }
        else
        {
            StopAllParticleEffects();
        }
    }

    private void UpdateLighting(WeatherType weather)
    {
        switch (weather)
        {
            case WeatherType.Sunny:
                sunLight.color = sunnyLightColor;
                sunLight.intensity = 1f;
                break;
            case WeatherType.Rainy:
                sunLight.color = rainyLightColor;
                sunLight.intensity = 0.6f;
                break;
            case WeatherType.Snowy:
                sunLight.color = snowyLightColor;
                sunLight.intensity = 0.8f;
                break;
            case WeatherType.Windy:
                sunLight.color = windyLightColor;
                sunLight.intensity = 0.9f;
                break;
            case WeatherType.Cloudy:
                sunLight.color = cloudyLightColor;
                sunLight.intensity = 0.7f;
                break;
        }
    }

    private void UpdateParticleEffects(WeatherType weather)
    {
        StopAllParticleEffects();

        switch (weather)
        {
            case WeatherType.Rainy:
                rainParticleSystem.Play();
                break;
            case WeatherType.Snowy:
                snowParticleSystem.Play();
                break;
            case WeatherType.Windy:
                windParticleSystem.Play();
                break;
            case WeatherType.Cloudy:
                cloudParticleSystem.Play();
                break;
        }
    }

    private void StopAllParticleEffects()
    {
        rainParticleSystem.Stop();
        snowParticleSystem.Stop();
        windParticleSystem.Stop();
        cloudParticleSystem.Stop();
    }
}