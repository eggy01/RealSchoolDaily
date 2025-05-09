using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class DayNightSystem : MonoBehaviour
{
    [Header("Global Light Settings")]
    public Light2D globalLight;
    public Gradient lightColorGradient;
    [Range(0.1f, 5f)] public float transitionSpeed = 1f;

    [Header("Outdoor Settings")]
    public float summerDayLength = 16f;    // Summer day length (hours)
    public float winterDayLength = 8f;     // Winter day length (hours)
    public float maxOutdoorIntensity = 1f; // Max intensity for outdoor scenes
    public float minOutdoorIntensity = 0.2f; // Min intensity for outdoor scenes

    [Header("Indoor Settings")]
    public float indoorLightIntensity = 1f; // Fixed intensity for indoor scenes

    [Header("Street Light Settings")]
    public float lightActivationThreshold = 0.3f;
    public float streetLightTransitionTime = 2f;

    // Private variables
    private float targetIntensity;
    private Color targetColor;
    private float seasonLerpFactor;
    private Coroutine lightTransitionCoroutine;
    private SceneLightController lightController;
    private bool streetLightsOn;
    private bool isOutdoorScene = true;
    private float currentTimeOfDay;

    private void Awake()
    {
        // Singleton pattern
        if (FindObjectsOfType<DayNightSystem>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        // Initialize street light controller
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
        CheckSceneType();
        InitializeLightSettings();
        UpdateLightingImmediately();
    }

    private void CheckSceneType()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        isOutdoorScene = WeatherManager.Instance.IsOuterScene(currentSceneName);

        // Disable street lights in indoor scenes
        if (!isOutdoorScene && lightController != null)
        {
            lightController.SetAllLights(false);
        }
    }

    private void InitializeLightSettings()
    {
        // Smooth light color gradient
        lightColorGradient = new Gradient()
        {
            colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.05f, 0.05f, 0.2f), 0f),    // Midnight
                new GradientColorKey(new Color(0.2f, 0.2f, 0.4f), 0.23f),  // Pre-dawn
                new GradientColorKey(new Color(1f, 0.7f, 0.5f), 0.25f),    // Sunrise
                new GradientColorKey(new Color(1f, 0.95f, 0.9f), 0.3f),    // Daytime
                new GradientColorKey(new Color(1f, 0.7f, 0.5f), 0.7f),     // Pre-sunset
                new GradientColorKey(new Color(0.2f, 0.2f, 0.4f), 0.75f),  // Sunset
                new GradientColorKey(new Color(0.05f, 0.05f, 0.2f), 0.77f) // Night
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
        UpdateLightingSmoothly();
    }

    private void OnSceneLoaded()
    {
        CheckSceneType();
        UpdateLightingImmediately();
    }

    private void UpdateLightingImmediately()
    {
        CalculateTargetLightValues();
        ApplyLightingImmediately();

        // Only update street lights in outdoor scenes
        if (isOutdoorScene)
        {
            UpdateStreetLights();
        }
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
        if (!isOutdoorScene)
        {
            // Indoor scene - fixed values
            targetIntensity = indoorLightIntensity;
            targetColor = Color.white;
            return;
        }

        // Outdoor scene - dynamic calculation
        Season currentSeason = TimeManager.Instance.GetSeason();
        seasonLerpFactor = CalculateSeasonLerpFactor(currentSeason, TimeManager.Instance.GetMonth());

        float currentDayLength = Mathf.Lerp(winterDayLength, summerDayLength, seasonLerpFactor);

        // Calculate current time of day (0-1)
        float currentHour = TimeManager.Instance.GetHour() + TimeManager.Instance.GetMinute() / 60f;
        currentTimeOfDay = currentHour / 24f;

        // Calculate day/night transition points
        float dawnTime = (12f - currentDayLength / 2f) / 24f;
        float sunriseTime = dawnTime + 0.05f;
        float sunsetTime = (12f + currentDayLength / 2f - 0.05f) / 24f;
        float duskTime = (12f + currentDayLength / 2f) / 24f;

        // Calculate target intensity (between minOutdoorIntensity and maxOutdoorIntensity)
        if (currentTimeOfDay <= dawnTime || currentTimeOfDay >= duskTime)
        {
            targetIntensity = minOutdoorIntensity; // Night time
        }
        else if (currentTimeOfDay <= sunriseTime)
        {
            float t = (currentTimeOfDay - dawnTime) / (sunriseTime - dawnTime);
            targetIntensity = Mathf.Lerp(minOutdoorIntensity, maxOutdoorIntensity, t);
        }
        else if (currentTimeOfDay >= sunsetTime)
        {
            float t = (duskTime - currentTimeOfDay) / (duskTime - sunsetTime);
            targetIntensity = Mathf.Lerp(minOutdoorIntensity, maxOutdoorIntensity, t);
        }
        else
        {
            targetIntensity = maxOutdoorIntensity; // Day time
        }

        // Calculate target color
        targetColor = lightColorGradient.Evaluate(currentTimeOfDay);
    }

    private float CalculateSeasonLerpFactor(Season season, int month)
    {
        if (season == Season.夏天) return 1f;
        if (season == Season.冬天) return 0f;

        if (season == Season.春天)
            return (month - 3) / 3f; // March=0, June=1
        else // Autumn
            return 1 - (month - 9) / 3f; // September=1, December=0
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

            if (isOutdoorScene)
            {
                UpdateStreetLights();
            }

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
        if (lightController == null || !isOutdoorScene) return;

        bool shouldLightsBeOn = globalLight.intensity <= lightActivationThreshold;

        if (shouldLightsBeOn != streetLightsOn)
        {
            streetLightsOn = shouldLightsBeOn;
            lightController.SetAllLights(streetLightsOn);

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