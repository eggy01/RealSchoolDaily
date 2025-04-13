using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings
{
    public const float itemfadeDuration = 0.25f;
    public const float fadeAlpha = 0.5f;

    public const float minuteThreshold = 1f; // 现实1秒=游戏1分钟 越小流速越快
    public const int secondHold = 59;
    public const int minuteHold = 59;
    public const int hourHold = 23;
    public const int seasonHold = 3;
    public const float PreSpringRainProb = 0.5f;//早春雨天概率
    public const float PreSummerSunProb = 0.7f;//初夏晴天概率
    public const float PreFullWindProb = 0.5f;//早秋风概率

    public const float fadeDuration = 0.5f;//场景切换
    public const float blackoutDuration = 0.5f;//场景切换时黑屏停留时间
    public const float checkInterval = 5f;//npc对话定期检测
    public static readonly Color DialogueInactiveColor = new Color(0.3f, 0.3f, 0.3f); // 对话图片未激活颜色（灰色）
}

