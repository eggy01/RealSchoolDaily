using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TipController : MonoBehaviour
{
    public static TipController Instance { get; private set; }
    [Header("UI References")]
    [SerializeField] public GameObject tipPanel;          // 通用提示面板
    [SerializeField] public TextMeshProUGUI tipText;     // 提示文本
    [SerializeField] public Animator tipAnimator;        // 提示动画控制器

    [SerializeField] private AudioSource audioSource;     // 音频源组件
    [SerializeField] private AudioClip showSound;         // 提示显示时的音效
    public UnityEngine.UI.Image TipIcon;//图标
    public Sprite[] IconSprites;
    //1，成就 2，任务 3， 4，物品好感度

    // [Header("Settings")]
    // [SerializeField] private float defaultShowTime = 1.5f; // 默认显示时长
    // [SerializeField] private string showTrigger = "Show"; // 显示动画触发器
    // [SerializeField] private string hideTrigger = "Hide";  // 隐藏动画触发器

    private bool isShowing = false;
    private Coroutine currentCoroutine;

    private void Awake()
    {
        Instance = this;
        // 确保初始状态
        //if (tipPanel != null) tipPanel.SetActive(false);
        // 如果没有指定AudioSource，尝试获取
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    // 通用显示方法
    public void ShowTip(string message, int InconNo)
    {
        if (isShowing)
        {
            StopCoroutine(currentCoroutine);
        }
        try
        {
            //Debug.Log("图标序号：" + InconNo);
            switch (InconNo)
            {
                case 1:
                    TipIcon.sprite = IconSprites[0];
                    break;
                case 2:
                    TipIcon.sprite = IconSprites[1];
                    break;
                case 3:
                    TipIcon.sprite = IconSprites[2];
                    break;
                case 4:
                    TipIcon.sprite = IconSprites[3];
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"图标: {e.Message}");
        }

        currentCoroutine = StartCoroutine(ShowTipCoroutine(message));
    }

    // 显示任务提示的快捷方法
    public void ShowTaskTip(bool isNewTask)
    {
        string message = isNewTask ? "解锁新任务！" : "任务完成！";
        ShowTip(message, 2);
    }

    // // 显示奖励提示的快捷方法
    // public void ShowRewardTip(string rewardName, int amount)
    // {
    //     string message = $"获得奖励: {rewardName} x{amount}";
    //     ShowTip(message,);
    // }

    private IEnumerator ShowTipCoroutine(string message, float displayTime = -1)
    {
        isShowing = true;
        //tipPanel.SetActive(true);
        Debug.Log("激活");

        // 播放音效
        if (showSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(showSound);
            Debug.Log("播放");
        }

        // 更新文本
        if (tipText != null) tipText.text = message;

        tipAnimator.SetBool("HasNewTip", true);
        yield return new WaitForSeconds(2f);
        tipAnimator.SetBool("HasNewTip", false);

        //禁用面板
        //if (tipPanel != null) tipPanel.SetActive(false);

        isShowing = false;
    }

    // 属性用于检查状态
    public bool IsShowingTip => isShowing;
}
