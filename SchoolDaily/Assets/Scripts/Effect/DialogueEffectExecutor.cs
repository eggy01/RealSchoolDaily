using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
//using SchoolD.Dialogue;
using UnityEngine.UIElements;
using System;
using static ScheduleEntry;
using static SchoolD.NewDialogue.DialogueData;
using System.Text.RegularExpressions;

public class DialogueEffectExecutor : MonoBehaviour
{
    // 单例实例
    public static DialogueEffectExecutor Instance { get; } = new DialogueEffectExecutor();
    public GameObject Player;

    // 效果类型分类器
    private readonly Dictionary<EffectType, System.Func<DialogueEffect, IEnumerator>> _effectHandlers;

    public DialogueEffectExecutor()
    {
        // 初始化效果处理器字典
        _effectHandlers = new Dictionary<EffectType, System.Func<DialogueEffect, IEnumerator>>
        {
            { EffectType.BlackScreen, ExecuteBlackScreen },
            { EffectType.TimeSkip, ExecuteTimeSkip },
            { EffectType.SceneTransition, ExecuteSceneTransition },
            { EffectType.MultipleEffects, ExecuteMultipleEffects },
            { EffectType.ShowText, ExecuteShowText }, // 新增文本显示效果类型
             { EffectType.PlaySound, ExecutePlaySound }, // 新增声音处理器
             { EffectType.PlayerAutoMoveto, ExecutePlayerAutoMoveto }, // 新增声音处理器
            { EffectType.RandomEvent, ExecuteRandomEvent },// 添加随机事件处理器
            { EffectType.AddNewChat, ExecuteAddNewChat }
        };
    }





    /// <summary>
    /// 执行效果列表
    /// </summary>
    public IEnumerator ExecuteEffects(List<DialogueEffect> effects)
    {
        if (effects == null || effects.Count == 0)
            yield break;

        Debug.Log($"开始处理效果列表，效果数量: {effects.Count}");
        foreach (var effect in effects)
        {
            Debug.Log($"- 类型: {effect.type}, 参数: {effect.parameters}");
        }

        yield return ExecuteEffectsSequentially(effects);
    }

    /// <summary>
    /// 分阶段顺序执行效果
    /// </summary>
    private IEnumerator ExecuteEffectsSequentially(List<DialogueEffect> effects)
    {
        // 执行准备阶段（黑屏淡入）
        yield return ExecuteEffectPhase(effects, IsPreparationEffect);

        // 执行核心阶段（确保所有效果完成）
        foreach (var effect in effects.Where(IsCoreEffect))
        {
            yield return ExecuteSingleEffect(effect); // 确保每个效果完全执行
        }

        // 执行收尾阶段（黑屏淡出）
        yield return ExecuteEffectPhase(effects, IsFinalizationEffect);
    }

    /// <summary>
    /// 执行特定阶段的效果
    /// </summary>
    private IEnumerator ExecuteEffectPhase(
        List<DialogueEffect> effects,
        System.Func<DialogueEffect, bool> phaseFilter)
    {
        foreach (var effect in effects.Where(phaseFilter))
        {
            yield return ExecuteSingleEffect(effect);
        }
    }

    /// <summary>
    /// 执行单个效果
    /// </summary>
    private IEnumerator ExecuteSingleEffect(DialogueEffect effect)
    {
        Debug.Log($"执行效果: {effect.type}, 参数: {effect.parameters}");

        if (_effectHandlers.TryGetValue(effect.type, out var handler))
        {
            yield return handler(effect);
        }
        else
        {
            Debug.LogWarning($"未知的效果类型: {effect.type}");
            yield return null;
        }
    }

    #region 具体效果实现

    private IEnumerator ExecuteBlackScreen(DialogueEffect effect)
    {
        bool isFadeIn = effect.parameters.Contains("淡入");

        if (isFadeIn)
        {
            // 黑屏淡入
            BlackScreenManager.Instance.TransionBlackScreenSortOrder(100);
            yield return BlackScreenManager.Instance.FadeIn(effect.duration, false);

            // 隐藏所有UI
            DialogueUI.Instance.SetAllFalse();

            Debug.Log("黑屏淡入完成");
        }
        else
        {
            // 黑屏淡出
            yield return BlackScreenManager.Instance.FadeOut(effect.duration, false);
            BlackScreenManager.Instance.TransionBlackScreenSortOrder(0);

            Debug.Log("黑屏淡出完成");
        }
    }

    private IEnumerator ExecuteTimeSkip(DialogueEffect effect)
    {
        TimeManager.Instance.SkipToTime(effect.parameters);
        yield return null;
        Debug.Log($"时间跳转到: {effect.parameters}");
    }

    private IEnumerator ExecuteSceneTransition(DialogueEffect effect)
    {
        var transitionData = ParseSceneTransition(effect.parameters);
        Debug.Log("场景切换数据：" + transitionData.sceneName + "X:" + transitionData.position.x + "Y:" + transitionData.position.y);
        EventHandler.CallTransitionEvent(transitionData.sceneName, transitionData.position);
        yield return null;
    }

    private IEnumerator ExecutePlayerAutoMoveto(DialogueEffect effect)
    {
        // 1. 获取目标位置
        Vector2 targetPos = ParsePosition(effect.parameters);

        // 2. 隐藏对话框
        DialogueUI.Instance.SetAllFalse();
        Debug.Log("对话框已隐藏，开始自动移动");

        // 3. 开始移动
        PlayerAutoMovement.MoveToPosition(targetPos);

        // 4. 等待移动完成
        PlayerAutoMovement movement = PlayerAutoMovement.FindPlayer()?.GetComponent<PlayerAutoMovement>();
        if (movement != null)
        {
            yield return new WaitWhile(() => movement.IsMoving());
            yield return new WaitForSeconds(1f);
            Debug.Log("自动移动完成");
        }
        else
        {
            Debug.LogError("找不到PlayerAutoMovement组件");
        }

        // 5. 重新显示对话框
        // DialogueUI.Instance.ShowDialogue();
        // Debug.Log("对话框已重新显示");

        // 6. 继续执行后续剧情
        yield return null;
    }

    private IEnumerator ExecuteShowText(DialogueEffect effect)
    {
        // 在黑屏状态下显示文本
        // 使用 yield return 等待动画完成
        yield return BlackScreenManager.Instance.AnimateText(effect.parameters);

        // 等待文本显示完成（假设每个字符显示0.05秒）
        float displayTime = effect.parameters.Length * 0.03f + 1f; // 额外5秒阅读时间
        yield return new WaitForSeconds(displayTime);

        Debug.Log($"文本显示完成: {effect.parameters}");
    }

    private IEnumerator ExecuteRandomEvent(DialogueEffect effect)
    {

        string[] parameters = effect.parameters.Split('，');
        if (parameters.Length == 0)
        {
            if (effect.parameters.Contains("&&"))
            {
                // 使用正则表达式匹配
                var match = Regex.Match(effect.parameters, @"(.+?)&&(\d+)%判定(\d+)\|(\d+):(.+)");

                if (match.Success)
                {
                    string condition = match.Groups[1].Value;  // "才艺≥10"
                    string probability = match.Groups[2].Value; // "90"
                    string successTarget = match.Groups[3].Value; // "8"
                    string failureTarget = match.Groups[4].Value; // "9"
                    string description = match.Groups[5].Value; // "string"

                    Debug.Log($"条件: {condition}");
                    Debug.Log($"概率: {probability}%");
                    Debug.Log($"成功跳转: {successTarget}");
                    Debug.Log($"失败跳转: {failureTarget}");
                    Debug.Log($"描述: {description}");
                    if (ConditionSystem.Check(condition))
                        EventHandler.CallLoadDialogueByIndex(successTarget, description);
                    else if (UnityEngine.Random.Range(0, 100) < 10)
                        EventHandler.CallLoadDialogueByIndex(failureTarget, description);
                    else EventHandler.CallLoadDialogueByIndex(successTarget, description);
                    yield break;
                }
                else
                {
                    Debug.LogError("格式解析失败");
                }
            }
            Debug.LogWarning("随机事件参数格式错误");
            yield break;
        }


        // 解析参数中的多个事件（用冒号分隔）
        string[] eventChain = effect.parameters.Split('：');

        foreach (string eventParam in eventChain)
        {
            string[] currentParams = eventParam.Split('，');

            if (currentParams.Length == 0) continue;

            // 格式1: 随机事件:R002（触发单个事件）
            if (currentParams.Length == 1)
            {
                string eventId = currentParams[0].Trim();
                bool eventCompleted = false;
                bool eventResult = false;

                Debug.Log("触发事件：" + eventId);
                RandomEventSystem.Instance.TriggerEvent(eventId, (result) =>
                {
                    eventResult = result;
                    eventCompleted = true;
                });

                while (!eventCompleted) yield return null;

                if (eventResult)
                {
                    TipController.Instance.ShowTip("失败", -1);
                    Debug.Log("触发成功: " + eventId);
                    yield break;  // 任一事件触发成功则终止整个流程
                }
                else
                {
                    Debug.Log("触发失败: " + eventId + "，尝试下一个事件");
                    continue;  // 继续尝试下一个事件
                }
            }
            // 格式2: 随机事件:R002,3（从R002组触发3个不同事件）
            else if (currentParams.Length == 2 && int.TryParse(currentParams[1], out int count))
            {
                string eventPrefix = currentParams[0].Trim();
                bool eventsCompleted = false;
                bool anyTriggered = false;

                RandomEventSystem.Instance.TriggerRandomEventsFromGroup(eventPrefix, count, (result) =>
                {
                    anyTriggered = result;
                    eventsCompleted = true;
                });

                while (!eventsCompleted) yield return null;

                if (anyTriggered)
                {
                    Debug.Log("触发成功: " + eventPrefix + "组中的" + count + "个事件");
                    yield break;  // 任一事件触发成功则终止整个流程
                }
                else
                {
                    Debug.Log("触发失败: " + eventPrefix + "组中的" + count + "个事件");
                    continue;  // 继续尝试下一个事件
                }
            }
        }

        // 所有事件都触发失败后继续执行后续剧情
        yield return null;
    }

    private IEnumerator ExecuteAddNewChat(DialogueEffect effect)
    {
        ChatSystem.Instance.ReceiveDeferredStory(effect.parameters);
        yield return null;
    }

    private IEnumerator ExecuteMultipleEffects(DialogueEffect effect)
    {
        if (!string.IsNullOrEmpty(effect.parameters))
        {
            var subEffects = ParseSubEffects(effect.parameters);
            yield return ExecuteEffects(subEffects);
        }
    }

    // 新增声音播放方法
    private IEnumerator ExecutePlaySound(DialogueEffect effect)
    {
        // 解析参数格式："音效名称|音量=0.8|循环=false"
        var soundParams = ParseSoundParameters(effect.parameters);

        Debug.Log($"播放音效: {soundParams.soundName}, 音量: {soundParams.volume}, 循环: {soundParams.loop}");

        // 调用音频管理器播放音效
        AudioManager.Instance.PlaySFX(soundParams.soundName, soundParams.volume, soundParams.loop);

        // 如果音效需要等待播放完成（如下课铃）
        if (soundParams.waitForCompletion)
        {
            yield return new WaitForSeconds(AudioManager.Instance.GetSoundDuration(soundParams.soundName));
        }
        else
        {
            yield return null;
        }
    }

    // 解析声音参数
    private (string soundName, float volume, bool loop, bool waitForCompletion) ParseSoundParameters(string parameters)
    {
        string soundName = parameters;
        float volume = 1.0f;
        bool loop = false;
        bool wait = false;

        // 支持多种参数格式：
        // 1. 简单格式："下课铃"
        // 2. 完整格式："下课铃，音量=0.8，循环=true，等待完成"

        if (parameters.Contains("，"))
        {
            var parts = parameters.Split('，');
            soundName = parts[0].Trim();

            foreach (var param in parts.Skip(1))
            {
                if (param.Contains("音量="))
                {
                    float.TryParse(param.Replace("音量=", ""), out volume);
                }
                else if (param.Contains("循环="))
                {
                    bool.TryParse(param.Replace("循环=", ""), out loop);
                }
                else if (param.Contains("等待完成"))
                {
                    wait = true;
                }
            }
        }

        return (soundName, volume, loop, wait);
    }


    #endregion

    #region 辅助方法

    private (string sceneName, Vector2 position) ParseSceneTransition(string parameters)
    {
        string[] parts = parameters.Split('，');
        return (
            sceneName: parts.Length >= 1 ? parts[0] : string.Empty,
            position: parts.Length > 1 ? ParsePosition(parts[1]) : Vector2.zero
        );
    }

    public List<DialogueEffect> ParseSubEffects(string parameters)
    {
        var results = new List<DialogueEffect>();

        foreach (var part in parameters.Split('|'))
        {
            if (string.IsNullOrWhiteSpace(part)) continue;

            var effect = new DialogueEffect();

            if (part.StartsWith("黑屏"))
            {
                effect.type = EffectType.BlackScreen;
                effect.parameters = part.Replace("黑屏", "").Trim();
                if (effect.parameters.Contains(","))
                {
                    var parts = effect.parameters.Split(',');
                    effect.parameters = parts[0].Trim();
                    if (float.TryParse(parts[1], out float dur))
                    {
                        effect.duration = dur;
                    }
                }
            }
            else if (part.StartsWith("显示文本="))
            {
                effect.type = EffectType.ShowText;
                effect.parameters = part.Replace("显示文本=", "").Trim();
            }
            else if (part.StartsWith("跳转时间:"))
            {
                effect.type = EffectType.TimeSkip;
                effect.parameters = part.Replace("跳转时间:", "").Trim();
            }
            else if (part.StartsWith("场景切换:"))
            {
                effect.type = EffectType.SceneTransition;
                effect.parameters = part.Replace("场景切换:", "");
            }
            else if (part.StartsWith("播放声音:"))
            {
                effect.type = EffectType.PlaySound;
                effect.parameters = part.Replace("播放声音:", "").Trim();
            }
            else if (part.StartsWith("自动移动:"))
            {
                Debug.Log("有自动移动");
                effect.type = EffectType.PlayerAutoMoveto;
                effect.parameters = part.Replace("自动移动:", "").Trim();
            }
            else if (part.StartsWith("随机事件:"))
            {
                Debug.Log("有随机事件");
                effect.type = EffectType.RandomEvent;
                effect.parameters = part.Replace("随机事件:", "").Trim();
            }
            else if (part.StartsWith("添加新聊天:"))
            {
                Debug.Log("添加新聊天:");
                effect.type = EffectType.AddNewChat;
                effect.parameters = part.Replace("添加新聊天:", "").Trim();
            }
            Debug.Log("效果+1");
            results.Add(effect);
        }

        return results;
    }

    private Vector2 ParsePosition(string posStr)
    {
        string[] coords = posStr.Split(';');
        if (coords.Length == 2 &&
            float.TryParse(coords[0], out float x) &&
            float.TryParse(coords[1], out float y))
        {
            return new Vector2(x, y);
        }
        return Vector2.zero;
    }

    private bool IsPreparationEffect(DialogueEffect effect)
    {
        return effect.type == EffectType.BlackScreen &&
               effect.parameters.Contains("淡入");
    }

    private bool IsCoreEffect(DialogueEffect effect)
    {
        return effect.type == EffectType.TimeSkip ||
               effect.type == EffectType.SceneTransition ||
               effect.type == EffectType.ShowText ||
               effect.type == EffectType.PlayerAutoMoveto ||
               effect.type == EffectType.RandomEvent;
    }

    private bool IsFinalizationEffect(DialogueEffect effect)
    {
        return effect.type == EffectType.BlackScreen &&
               effect.parameters.Contains("淡出");
    }

    #endregion
}
