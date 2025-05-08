using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
//using UnityEngine.InputSystem; // 使用Unity的新输入系统

public class BeginAnimManager : MonoBehaviour
{
    public static BeginAnimManager Instance { get; set; }
    public PlayableDirector PlayableDirector;//开场动画TimeLine
    public GameObject player;
    public static bool isPlaying = false;//标志是否播放过开场动画
    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public IEnumerator PlayNewBeginAnim()
    {
        //yield return StartCoroutine(ShowAcceptTanceLetter());
        PlayableDirector.Play();
        // 等待动画播放完成
        yield return new WaitForSeconds((float)PlayableDirector.duration);
        PlayableDirector.enabled = false; // 禁用PlayableDirector
        SetPlayerPosition();

        isPlaying = true;//标志已过过开场动画，防止下次进入在播放
    }

    public IEnumerator ShowAcceptTanceLetter()
    {
        BlackScreenManager.Instance.TransionBlackScreenSortOrder(1000);
        yield return BlackScreenManager.Instance.FadeIn(0.5f, false);
        player.gameObject.SetActive(false);

        BlackScreenManager.Instance.ShowImage(true);//显示图片
                                                    // 等待一段时间才允许跳过
        yield return new WaitForSeconds(3f);

        // 等待玩家按下任意键

        yield return BlackScreenManager.Instance.FadeOut(1f, false);
        BlackScreenManager.Instance.TransionBlackScreenSortOrder(0);
        BlackScreenManager.Instance.ShowImage(false);//显示图片
    }
    public void SetPlayerPosition()//设置玩家初始位置
    {
        player.transform.position = new Vector3(13.5f, 34.7f, 0);
        player.gameObject.SetActive(true);
    }
}
