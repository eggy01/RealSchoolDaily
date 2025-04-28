using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioClip[] soundEffects;

    private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            sfxSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySFX(string soundName, float volume = 1f, bool loop = false)
    {
        var clip = System.Array.Find(soundEffects, s => s.name == soundName);
        if (clip != null)
        {
            sfxSource.clip = clip;
            sfxSource.volume = volume;
            sfxSource.loop = loop;
            sfxSource.Play();
        }
        else
        {
            Debug.LogWarning($"音效未找到: {soundName}");
        }
    }

    public float GetSoundDuration(string soundName)
    {
        var clip = System.Array.Find(soundEffects, s => s.name == soundName);
        return clip != null ? clip.length : 0f;
    }
}