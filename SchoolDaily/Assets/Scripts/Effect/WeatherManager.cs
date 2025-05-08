using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance { get; private set; }
    public enum WeatherType
    {
        Sunny,
        Rainy,
        Snowy,
        // Windy,
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
    public static bool isOutdoorScene { get; private set; }

    [Header("光照设置")]
    public Light2D sunLight2D; // 改为使用Light2D组件
    public Color sunnyLightColor = new Color(1f, 1f, 0.95f);
    public Color rainyLightColor = new Color(0.7f, 0.7f, 0.8f);
    public Color snowyLightColor = new Color(0.9f, 0.9f, 1f);
    //public Color windyLightColor = new Color(0.85f, 0.85f, 0.75f);
    public Color cloudyLightColor = new Color(0.6f, 0.6f, 0.6f);


    [Header("天气UI")]
    public Image WeatherUI;
    public Sprite[] WeatherSprites;

    [Header("天气参数")]
    public float minWeatherDuration = 1f;
    public float maxWeatherDuration = 3f;
    private float currentWeatherDuration;
    private int daysPassedWithCurrentWeather;
    public float transitionDuration = 5f;

    [Header("粒子系统")]
    public ParticleSystem rainParticleSystem;
    public ParticleSystem snowParticleSystem;
    //public ParticleSystem windParticleSystem;
    //public ParticleSystem cloudParticleSystem;

    private WeatherType currentWeather;
    private TimeManager timeManager;


    private void Awake()
    {
        Instance = this;
        timeManager = FindObjectOfType<TimeManager>();
        CheckCurrentScene();
    }

    private void Start()
    {
        currentWeatherDuration = UnityEngine.Random.Range(minWeatherDuration, maxWeatherDuration);
        daysPassedWithCurrentWeather = 0;
        ChangeRandomWeather();
    }

    private void OnEnable()
    {
        EventHandler.OnDateChanged += OnDayChanged;
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnDisable()
    {
        EventHandler.OnDateChanged -= OnDayChanged;
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        CheckCurrentScene();
        UpdateWeatherEffects();
    }

    public bool IsOuterScene(string scenename)//判断是否为室外场景
    {
        return outdoorScenes.Contains(scenename);
    }

    private void CheckCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        isOutdoorScene = outdoorScenes.Contains(currentScene);
    }

    private void OnDayChanged(string date)
    {
        if (!timeManager.gameClockPause)
        {
            daysPassedWithCurrentWeather++;

            if (daysPassedWithCurrentWeather >= currentWeatherDuration)
            {
                ChangeRandomWeather();
                daysPassedWithCurrentWeather = 0;
                currentWeatherDuration = UnityEngine.Random.Range(minWeatherDuration, maxWeatherDuration);
            }
        }
    }

    //更新天气UI
    private void UpdateWeatherIcon(WeatherType weather)
    {
        if (WeatherUI == null || WeatherSprites == null || WeatherSprites.Length < 4)
        {
            return;
        }

        int index = weather switch
        {
            WeatherType.Cloudy => 0,
            WeatherType.Rainy => 1,
            WeatherType.Snowy => 2,
            WeatherType.Sunny => 3,
            _ => 2 // 默认返回Sunny
        };

        if (index < WeatherSprites.Length && WeatherSprites[index] != null)
        {
            WeatherUI.sprite = WeatherSprites[index];
        }
    }

    private void ChangeRandomWeather()
    {
        WeatherType newWeather = GetSeasonalWeather();
        UpdateWeatherIcon(newWeather);
        SetWeather(newWeather);
    }

    private void UpdateWeatherEffects()
    {
        // 根据当前是否在室外场景重新激活/禁用天气效果
        SetWeather(currentWeather);
    }

    private WeatherType GetSeasonalWeather()
    {
        float rand = UnityEngine.Random.value;

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
                    if (rand < 0.5f) return WeatherType.Rainy;
                    if (rand < 0.7f) return WeatherType.Cloudy;
                    return WeatherType.Sunny;
                }
                else // 深秋
                {
                    if (rand < 0.6f) return WeatherType.Sunny;
                    if (rand < 0.8f) return WeatherType.Cloudy;
                    return WeatherType.Rainy;
                }

            case Season.冬天: // 12-2月
                if (month == 12) // 初冬
                {
                    if (rand < 0.6f) return WeatherType.Snowy;
                    if (rand < 0.8f) return WeatherType.Cloudy;
                    return WeatherType.Sunny;
                }
                else // 深冬
                {
                    if (rand < 0.7f) return WeatherType.Snowy;
                    if (rand < 0.85f) return WeatherType.Cloudy;
                    return WeatherType.Sunny;
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
        if (sunLight2D == null)
        {
            //Debug.LogWarning("未找到2D光源组件");
            return;
        }

        switch (weather)
        {
            case WeatherType.Sunny:
                sunLight2D.color = sunnyLightColor;
                //sunLight2D.intensity = 1f;
                break;
            case WeatherType.Rainy:
                sunLight2D.color = rainyLightColor;
                //sunLight2D.intensity = 0.6f;
                break;
            case WeatherType.Snowy:
                sunLight2D.color = snowyLightColor;
                //sunLight2D.intensity = 0.8f;
                break;
            // case WeatherType.Windy:
            //     sunLight2D.color = windyLightColor;
            //     //sunLight2D.intensity = 0.9f;
            //     break;
            case WeatherType.Cloudy:
                sunLight2D.color = cloudyLightColor;
                //sunLight2D.intensity = 0.7f;
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
                // case WeatherType.Windy:
                //     windParticleSystem.Play();
                //     break;
                // case WeatherType.Cloudy:
                //     cloudParticleSystem.Play();
                //     break;
        }
    }

    private void StopAllParticleEffects()
    {
        rainParticleSystem.Stop();
        snowParticleSystem.Stop();
        //windParticleSystem.Stop();
        // cloudParticleSystem.Stop();
    }
}