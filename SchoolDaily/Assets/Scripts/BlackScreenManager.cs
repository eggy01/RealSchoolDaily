using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlackScreenManager : MonoBehaviour
{
    public static BlackScreenManager Instance { get; private set; }

    [SerializeField] private CanvasGroup blackScreenCanvasGroup;
    [SerializeField] private TextMeshProUGUI textElement; // 手动拖拽赋值

    private void Awake()
    {
        Instance = this;
    }
    public void TransionBlackScreenSortOrder(int num)//调整黑屏层级
    {
        blackScreenCanvasGroup.GetComponentInParent<Canvas>().sortingOrder = num;
    }
    /// <summary>
    /// 执行完整的黑屏过渡效果（淡入→保持→淡出）
    /// </summary>
    public IEnumerator PlayTransition(float fadeDuration, float holdDuration, bool showText = false)
    {
        // Canvas parentCanvas = blackScreenCanvasGroup.GetComponentInParent<Canvas>();
        // parentCanvas.sortingOrder = 100;
        TransionBlackScreenSortOrder(100);
        // 淡入
        yield return FadeIn(fadeDuration, showText);

        // 保持黑屏
        yield return new WaitForSeconds(holdDuration);

        // 淡出
        yield return FadeOut(fadeDuration, showText);
        TransionBlackScreenSortOrder(0);
        // parentCanvas.sortingOrder = 2;
    }
    /// <summary>
    /// 淡入黑屏
    /// </summary>
    public IEnumerator FadeIn(float duration, bool showText)
    {
        SetTextVisibility(showText);
        float targetAlpha = 1f;
        float speed = Mathf.Abs(blackScreenCanvasGroup.alpha - targetAlpha) / duration;

        while (!Mathf.Approximately(blackScreenCanvasGroup.alpha, targetAlpha))
        {
            blackScreenCanvasGroup.alpha = Mathf.MoveTowards(blackScreenCanvasGroup.alpha, targetAlpha, speed * Time.deltaTime);
            yield return null;
        }
    }

    /// <summary>
    /// 淡出黑屏
    /// </summary>
    public IEnumerator FadeOut(float duration, bool showText)
    {
        SetTextVisibility(showText);

        float targetAlpha = 0f;
        float speed = Mathf.Abs(blackScreenCanvasGroup.alpha - targetAlpha) / duration;

        while (!Mathf.Approximately(blackScreenCanvasGroup.alpha, targetAlpha))
        {
            blackScreenCanvasGroup.alpha = Mathf.MoveTowards(blackScreenCanvasGroup.alpha, targetAlpha, speed * Time.deltaTime);
            yield return null;
        }
    }
    /// <summary>
    /// 设置文本显示状态
    /// </summary>
    private void SetTextVisibility(bool show)
    {
        if (textElement != null)
            textElement.gameObject.SetActive(show);
    }
}
