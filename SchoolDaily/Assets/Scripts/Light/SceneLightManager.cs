// SceneLightManager.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLightManager : MonoBehaviour
{
    public static SceneLightManager Instance;

    [System.Serializable]
    public class SceneLightSetup
    {
        public string sceneName;
        public Transform lightsParent;
    }

    [Header("场景灯光配置")]
    public List<SceneLightSetup> sceneLightSetups = new List<SceneLightSetup>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Transform GetLightParentForScene(string sceneName)
    {
        return sceneLightSetups.Find(x => x.sceneName == sceneName)?.lightsParent;
    }
}