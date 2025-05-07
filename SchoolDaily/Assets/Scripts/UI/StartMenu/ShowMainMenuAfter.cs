using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator openingAnimator;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private string animationName = "OpeningAnimation";

    private static bool hasPlayedOpening = false;

    void Start()
    {
        if (!hasPlayedOpening)
        {
            // 首次启动时播放动画
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);

            StartCoroutine(PlayOpeningSequence());
            hasPlayedOpening = true;
        }
        else
        {
            // 非首次启动时直接显示菜单
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);

            openingAnimator.gameObject.SetActive(false);
        }
    }

    IEnumerator PlayOpeningSequence()
    {
        // 等待动画初始化
        yield return null;

        // 获取动画时长
        float animLength = (float)(openingAnimator.GetCurrentAnimatorStateInfo(0).length - 0.1);
        yield return new WaitForSeconds(animLength);
        // 动画结束后显示菜单
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        openingAnimator.gameObject.SetActive(false);
        // // 播放动画
        // openingAnimator.Play(animationName);

        // // 等待动画结束
        // yield return new WaitForSeconds(animLength);

        // // 显示主菜单
        // if (mainMenuPanel != null)
        //     mainMenuPanel.SetActive(true);

        // // 可选：禁用动画对象
        // openingAnimator.gameObject.SetActive(false);
    }

    //如果需要在退出游戏时重置状态（根据需求可选）
    private void OnApplicationQuit()
    {
        hasPlayedOpening = false;
    }
}