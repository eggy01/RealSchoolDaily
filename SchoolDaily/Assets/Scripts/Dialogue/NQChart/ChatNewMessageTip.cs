using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatNewMessageTip : MonoBehaviour
{
    public static ChatNewMessageTip Instance { get; set; }
    // 添加这些变量到您的类中
    [Header("Notification Settings")]
    [SerializeField] private AudioClip messageNotificationSound;
    [SerializeField] private GameObject notificationBadge; // 主界面上的红点图标
    [SerializeField] private float notificationDuration = 2f;

    private AudioSource audioSource;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 显示新消息通知（音效+红点）
    /// </summary>
    public void ShowNewMessageNotification()
    {
        // 播放提示音
        PlayNotificationSound();

        // 显示红点
        ShowNotificationBadge();

        // 可选：自动隐藏红点
        // Invoke("HideNotificationBadge", notificationDuration);
    }

    /// <summary>
    /// 播放消息提示音
    /// </summary>
    private void PlayNotificationSound()
    {
        // 调用音频管理器播放音效
        AudioManager.Instance.PlaySFX("新消息提示音");

    }

    /// <summary>
    /// 显示通知红点
    /// </summary>
    private void ShowNotificationBadge()
    {
        if (notificationBadge != null)
        {
            notificationBadge.SetActive(true);
        }
    }

    /// <summary>
    /// 隐藏通知红点
    /// </summary>
    public void HideNotificationBadge()
    {
        if (notificationBadge != null)
        {
            notificationBadge.SetActive(false);
        }
    }
}
