using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Expression
{
    [Header("基础设置")]
    public string expressionID;      // 表情标识（如"happy", "angry"）
    public Sprite sprite;           // 表情贴图

    [Header("动画效果")]
    public float duration = 0.5f;   // 持续时间
    public Vector2 offset;          // 表情偏移量
    public float scale = 1f;        // 缩放比例

    [Header("音效")]
    public AudioClip soundEffect;    // 表情音效

    // 表情混合模式（用于复杂表情叠加）
    public enum BlendMode
    {
        Replace,    // 替换基础立绘
        Overlay,    // 叠加在立绘上
        Combine     // 与基础立绘混合
    }
    public BlendMode blendMode;
}

// 扩展方法 - 表情应用
public static class ExpressionExtensions
{
    public static void ApplyTo(this Expression exp, Image portraitImage)
    {
        if (exp.blendMode == Expression.BlendMode.Replace)
        {
            portraitImage.sprite = exp.sprite;
        }

        portraitImage.rectTransform.localPosition = exp.offset;
        portraitImage.rectTransform.localScale = Vector3.one * exp.scale;

        //音频控制
        // if (exp.soundEffect != null)
        // {
        //     AudioManager.PlaySfx(exp.soundEffect);
        // }
    }
}
