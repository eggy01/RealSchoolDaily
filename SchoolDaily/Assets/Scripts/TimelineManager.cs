using UnityEngine;
using UnityEngine.Playables;
using System.Collections;
using UnityEngine.Timeline;
using System;

public class TimelineManager : MonoBehaviour
{
    public static TimelineManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private PlayableDirector _sceneTimeline; // 直接拖拽场景中的Timeline对象到这里

    private bool _isTimelinePlaying;
    private Coroutine _timelineCoroutine;

    // 事件：Timeline开始/结束
    public static event Action OnTimelineStarted;
    public static event Action OnTimelineFinished;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// 播放场景中预设的Timeline
    /// </summary>
    public void PlayTimeline(bool pauseGameplay = true)
    {
        if (_sceneTimeline == null)
        {
            Debug.LogError("场景中没有设置Timeline对象！");
            return;
        }

        if (_isTimelinePlaying)
        {
            Debug.LogWarning("已有Timeline正在播放，强制终止！");
            StopCurrentTimeline();
        }

        _timelineCoroutine = StartCoroutine(PlayTimelineRoutine(pauseGameplay));
    }

    private IEnumerator PlayTimelineRoutine(bool pauseGameplay)
    {
        // 初始化Director
        _sceneTimeline.stopped += OnTimelineStopped;
        _isTimelinePlaying = true;
        OnTimelineStarted?.Invoke();

        // 播放Timeline
        _sceneTimeline.Play();

        // 安全等待
        while (_sceneTimeline != null && _sceneTimeline.state == PlayState.Playing)
        {
            yield return null;
        }

        // 兜底清理
        if (_sceneTimeline != null)
        {
            OnTimelineStopped(_sceneTimeline);
        }
    }

    /// <summary>
    /// 停止当前Timeline
    /// </summary>
    public void StopCurrentTimeline()
    {
        if (_sceneTimeline != null)
        {
            _sceneTimeline.Stop();
            CleanupDirector();
        }

        if (_timelineCoroutine != null)
        {
            StopCoroutine(_timelineCoroutine);
            _timelineCoroutine = null;
        }
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        CleanupDirector();
        OnTimelineFinished?.Invoke();
    }

    private void CleanupDirector()
    {
        if (_sceneTimeline != null)
        {
            _sceneTimeline.stopped -= OnTimelineStopped;
        }
        _isTimelinePlaying = false;
    }

    /// <summary>
    /// 动态绑定Timeline轨道
    /// </summary>
    public void BindTrack(string trackName, UnityEngine.Object bindingTarget)
    {
        if (_sceneTimeline == null) return;

        foreach (var output in _sceneTimeline.playableAsset.outputs)
        {
            if (output.streamName == trackName)
            {
                _sceneTimeline.SetGenericBinding(output.sourceObject, bindingTarget);
                break;
            }
        }
    }

    public bool IsTimelinePlaying() => _isTimelinePlaying;
}