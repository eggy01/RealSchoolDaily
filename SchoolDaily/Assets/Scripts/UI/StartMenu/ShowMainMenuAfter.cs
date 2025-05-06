using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator openingAnimator;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private string animationName = "OpeningAnimation";

    void Start()
    {
        // 确保主菜单隐藏
        if(mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        
        StartCoroutine(PlayOpeningSequence());
    }

    IEnumerator PlayOpeningSequence()
    {
        // 等待动画初始化
        yield return null;
        
        // 获取动画时长
        float animLength = (float)(openingAnimator.GetCurrentAnimatorStateInfo(0).length-0.1);
        
        // 播放动画
        openingAnimator.Play(animationName);
        
        // 等待动画结束
        yield return new WaitForSeconds(animLength);
        
        // 显示主菜单
        if(mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
        
        // 可选：禁用动画对象
        openingAnimator.gameObject.SetActive(false);
    }

    // 动画事件回调方法（如果使用动画事件）
    public void OnAnimationEnd()
    {
        if(mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
        
        openingAnimator.gameObject.SetActive(false);
    }
}