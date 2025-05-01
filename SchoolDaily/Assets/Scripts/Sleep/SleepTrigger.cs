using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SleepTrigger : MonoBehaviour
{
    public GameObject SleepTip;//睡觉提示框窗口
    private void Start()
    {
        SleepTip = ToolTipSystem.SleepTip;
        SleepTip.SetActive(true);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SleepTip.SetActive(true);
        }
    }
    IEnumerator sleep()
    {
        // 黑屏淡入
        BlackScreenManager.Instance.TransionBlackScreenSortOrder(100);
        yield return BlackScreenManager.Instance.FadeIn(0.5f, false);

        BlackScreenManager.Instance.SetText("安然入睡，你做了个好梦");
        yield return new WaitForSeconds(0.5f);
        // 黑屏淡出
        yield return BlackScreenManager.Instance.FadeOut(0.5f, false);
        BlackScreenManager.Instance.TransionBlackScreenSortOrder(0);
    }
    void NoSlepp()
    {
        SleepTip.SetActive(false);
    }
}
