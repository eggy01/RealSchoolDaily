using UnityEngine;
using System.Collections.Generic;

public class AllAudioManager : MonoBehaviour
{
    public static AllAudioManager Instance { get; private set; }

    [System.Serializable]
    public class SoundCategory
    {
        public string categoryName; // 分类名称（如"UI", "Footstep", "Environment"）
        public AudioClip[] clips;
        public float defaultVolume = 1f;
        public bool is3DSound = false;
    }

    [SerializeField] private SoundCategory[] soundCategories; // 分类配置

    private Dictionary<AudioSource, string> audioSourceCategories = new Dictionary<AudioSource, string>();
    private Dictionary<string, SoundCategory> categoryDict = new Dictionary<string, SoundCategory>();
    private List<AudioSource> activeSources = new List<AudioSource>(); // 活跃的音频源
    private GameObject audioSourcePool; // 音频源对象池父物体

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        // 初始化分类字典
        foreach (var category in soundCategories)
        {
            categoryDict[category.categoryName] = category;
        }

        // 创建对象池父物体
        audioSourcePool = new GameObject("AudioSourcePool");
        audioSourcePool.transform.SetParent(transform);
    }

    // 播放音效（推荐通过分类名称+音频名称访问）
    public void PlaySFX(string categoryName, string soundName, float volumeModifier = 1f, bool loop = false)
    {
        if (categoryDict.TryGetValue(categoryName, out SoundCategory category))
        {
            AudioClip clip = System.Array.Find(category.clips, c => c.name == soundName);
            if (clip != null)
            {
                AudioSource source = GetAvailableAudioSource();
                ConfigureAudioSource(source, category, clip, volumeModifier, loop);
                source.Play();
                if (!loop) StartCoroutine(ReturnToPoolAfterPlay(source, clip.length));
            }
            else
            {
                Debug.LogWarning($"音效未找到: {categoryName}/{soundName}");
            }
        }
        else
        {
            Debug.LogWarning($"分类未找到: {categoryName}");
        }
    }

    // 停止特定分类的所有音效
    public void StopCategory(string categoryName, bool fadeOut = false)
    {
        foreach (var source in activeSources)
        {
            if (source.isPlaying &&
            audioSourceCategories.TryGetValue(source, out string currentCategory) &&
            currentCategory == categoryName)
            {
                if (fadeOut)
                    StartCoroutine(FadeOutSource(source, 0.5f));
                else
                    source.Stop();
            }
        }
    }

    // 获取可用音频源（对象池技术）
    private AudioSource GetAvailableAudioSource()
    {
        foreach (var source in activeSources)
        {
            if (!source.isPlaying) return source;
        }

        // 没有可用音频源时创建新的
        GameObject newSourceObj = new GameObject("SFX Source");
        newSourceObj.transform.SetParent(audioSourcePool.transform);
        AudioSource newSource = newSourceObj.AddComponent<AudioSource>();
        activeSources.Add(newSource);
        return newSource;
    }

    // 配置音频源参数
    private void ConfigureAudioSource(AudioSource source, SoundCategory category, AudioClip clip,
                                 float volumeModifier, bool loop)
    {

        // 记录到字典
        audioSourceCategories[source] = category.categoryName;

        // 其他配置保持不变
        source.clip = clip;
        source.volume = category.defaultVolume * volumeModifier;
        source.loop = loop;
        source.spatialBlend = category.is3DSound ? 1f : 0f;
    }

    // 播放完毕后回收音频源
    private System.Collections.IEnumerator ReturnToPoolAfterPlay(AudioSource source, float duration)
    {
        yield return new WaitForSeconds(duration);
        source.Stop();
        source.clip = null;

        // 新增字典清理
        if (audioSourceCategories.ContainsKey(source))
        {
            audioSourceCategories.Remove(source);
        }
    }

    // 淡出效果协程
    private System.Collections.IEnumerator FadeOutSource(AudioSource source, float fadeDuration)
    {
        float startVolume = source.volume;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            source.volume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        source.Stop();
        source.volume = startVolume;
    }
}