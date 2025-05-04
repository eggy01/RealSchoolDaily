using UnityEngine;
using System.Collections.Generic;
using System;

public interface IWindow
{
    void Open(params object[] args);
    void Close();
    bool ShouldPauseTime { get; } // 窗口是否需要暂停时间
    bool ShouldPausePlayer { get; } // 窗口是否需要暂停人物
    bool IsOpen { get; }
}

public class WindowManager : MonoBehaviour
{
    public static WindowManager Instance;

    private List<IWindow> _openWindows = new List<IWindow>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // 窗口状态检查
    public bool IsWindowOpen(System.Type windowType)
    {
        foreach (var window in _openWindows)
        {
            if (window.GetType() == windowType)
            {
                return true;
            }
        }
        return false;
    }

    public void OpenWindow(IWindow window, params object[] args)
    {
        // 关闭所有已打开的窗口
        CloseAll();

        if (!_openWindows.Contains(window))
        {
            _openWindows.Add(window);
            window.Open(args);
        }

        UpdateGameState();
    }

    public void CloseWindow(IWindow window)
    {
        if (_openWindows.Remove(window))
        {
            window.Close();
        }
        UpdateGameState();
    }

    public void CloseAll(Func<IWindow, bool> predicate = null)
    {
        for (int i = _openWindows.Count - 1; i >= 0; i--)
        {
            if (predicate == null || predicate(_openWindows[i]))
            {
                _openWindows[i].Close();
                _openWindows.RemoveAt(i);
            }
        }
        UpdateGameState();
    }

    private void UpdateGameState()
    {
        // 分别检查是否有窗口需要暂停时间或暂停人物
        bool shouldPauseTime = _openWindows.Exists(w => w.ShouldPauseTime);
        bool shouldPausePlayer = _openWindows.Exists(w => w.ShouldPausePlayer);

        // 为时间暂停和人物暂停设置独立的暂停状态
        PauseManager.Instance.SetPauseState(shouldPauseTime);
        PlayerController.Instance.movement.SetPause(shouldPausePlayer);
    }
}