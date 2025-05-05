using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class CursorController : MonoBehaviour
{
    public static CursorController Instance;

    [Header("点击效果设置")]
    //public Texture2D clickCursor;
    public Vector2 hotspot = Vector2.zero;
    public CursorMode cursorMode = CursorMode.Auto;

    [Header("正常状态动画")]
    public Texture2D[] normalAnimationFrames;
    public float normalFrameInterval = 0.5f;
    private Coroutine normalAnimationCoroutine;

    [Header("悬停状态动画")]
    public Texture2D[] hoverAnimationFrames;
    public float hoverFrameInterval = 0.5f;
    private Coroutine hoverAnimationCoroutine;

    private bool isHovering;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartNormalAnimation();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        CheckHoverState();
    }

    private void CheckHoverState()
    {
        bool newHoverState = false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            Button btn = result.gameObject.GetComponentInParent<Button>();
            HoverCursorButton hoverBtn = result.gameObject.GetComponentInParent<HoverCursorButton>();

            if (btn != null && btn.interactable &&
                hoverBtn != null && hoverBtn.enableHoverEffect)
            {
                newHoverState = true;
                break; // 只响应最顶部的有效按钮
            }
        }

        if (newHoverState != isHovering)
        {
            isHovering = newHoverState;
            if (isHovering)
            {
                StartHoverAnimation();
            }
            else
            {
                StopHoverAnimation();
            }
        }
    }

    private void StartNormalAnimation()
    {
        if (normalAnimationFrames != null && normalAnimationFrames.Length > 0)
        {
            normalAnimationCoroutine = StartCoroutine(PlayAnimation(
                normalAnimationFrames,
                normalFrameInterval,
                texture => hotspot // 使用Inspector设置的公共hotspot
            ));
        }
    }

    private void StartHoverAnimation()
    {
        if (hoverAnimationFrames != null && hoverAnimationFrames.Length > 0)
        {
            if (normalAnimationCoroutine != null)
            {
                StopCoroutine(normalAnimationCoroutine);
                normalAnimationCoroutine = null;
            }
            hoverAnimationCoroutine = StartCoroutine(PlayAnimation(
                hoverAnimationFrames,
                hoverFrameInterval,
                texture => new Vector2(texture.width / 2f, 5f) // 顶部中间热点
            ));
        }
    }

    private void StopHoverAnimation()
    {
        if (hoverAnimationCoroutine != null)
        {
            StopCoroutine(hoverAnimationCoroutine);
            hoverAnimationCoroutine = null;
        }
        StartNormalAnimation();
    }

    private IEnumerator PlayAnimation(Texture2D[] frames, float interval, Func<Texture2D, Vector2> hotspotCalculator)
    {
        int currentFrame = 0;
        while (true)
        {
            Texture2D currentTexture = frames[currentFrame];
            Vector2 frameHotspot = hotspotCalculator(currentTexture);
            Cursor.SetCursor(currentTexture, frameHotspot, cursorMode);
            currentFrame = (currentFrame + 1) % frames.Length;
            yield return new WaitForSeconds(interval);
        }
    }

}