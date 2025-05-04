using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SleepTrigger : MonoBehaviour
{
    public GameObject SleepTip;//睡觉提示框窗口
    private void Start()
    {
        //SleepTip = ToolTipSystem.SleepTip;
        SleepTip.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController.Instance.movement.SetPause(true);
            SleepTip.SetActive(true);
        }
    }
    public void Slepp()
    {
        SleepTip.SetActive(false);
        StartCoroutine(EnterNextDay());
        PlayerController.Instance.movement.SetPause(false);
    }
    IEnumerator EnterNextDay()
    {
        // 黑屏淡入
        BlackScreenManager.Instance.TransionBlackScreenSortOrder(100);
        yield return BlackScreenManager.Instance.FadeIn(0.5f, false);

        BlackScreenManager.Instance.SetText("安然入睡，你做了个好梦");
        TimeManager.Instance.SkipToNextDay();
        yield return new WaitForSeconds(1f);

        yield return new WaitForSeconds(0.5f);
        // 黑屏淡出
        yield return BlackScreenManager.Instance.FadeOut(0.5f, false);
        BlackScreenManager.Instance.TransionBlackScreenSortOrder(0);
    }
    public void NoSlepp()
    {
        SleepTip.SetActive(false);
        PlayerController.Instance.movement.SetPause(false);
    }
}
