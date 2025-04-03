using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ItemFader : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void FadeIn()//逐渐恢复颜色
    {
        Color targetColor = new Color(1, 1, 1, 1);
        spriteRenderer.DOColor(targetColor, Settings.fadeDuration);
    }

    public void FadeOut()//逐渐半透明
    {
        Color targetColor = new Color(1, 1, 1, Settings.fadeAlpha);
        spriteRenderer.DOColor(targetColor, Settings.fadeDuration);
    }
}
