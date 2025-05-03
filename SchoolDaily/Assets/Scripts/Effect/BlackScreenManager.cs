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
        if (blackScreenCanvasGroup == null)
        {
            blackScreenCanvasGroup = GetComponent<CanvasGroup>();
            Debug.LogError("未分配blackScreenCanvasGroup，已自动获取");
        }
    }
    public void SetText(string str)
    {
        textElement.text = str;
        SetTextVisibility(true);
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

    public IEnumerator AnimateText(string text)
    {
        SetTextVisibility(true);

        float lettersPerSecond = text.Length / 0.5f;

        for (int i = 0; i <= text.Length; i++)
        {
            // 如果玩家按了空格或点击，立即显示完整文本
            if (Input.GetMouseButtonDown(0))
            {
                textElement.text = text;
                break;
            }

            textElement.text = text.Substring(0, i);
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }

        // 短暂停留后隐藏
        yield return new WaitForSeconds(0.5f);
        SetTextVisibility(false);
    }
}
