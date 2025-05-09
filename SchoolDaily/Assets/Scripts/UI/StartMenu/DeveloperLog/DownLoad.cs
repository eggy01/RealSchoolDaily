using UnityEngine;

public class DownLoad : MonoBehaviour
{
    public GameObject popupWindow;   // 需要弹出的窗口（初始应禁用）
    public GameObject persistentObject; // 需要永久显示的物体（初始应禁用）

    void Start()
    {
        // 首次启动检查
        if (!PlayerPrefs.HasKey("FirstLaunch"))
        {
            PlayerPrefs.SetInt("FirstLaunch", 1);
            PlayerPrefs.SetInt("PopupShown", 0); // 新增弹窗显示标识
            PlayerPrefs.Save();
            return;
        }

        // 非首次启动且弹窗未显示过
        if (PlayerPrefs.GetInt("PopupShown") == 0)
        {
            popupWindow.SetActive(true);
            persistentObject.SetActive(false);
        }
        else
        {
            persistentObject.SetActive(true);
        }
    }

    // 绑定到弹窗的确定按钮
    public void OnConfirmPopup()
    {
        popupWindow.SetActive(false);
        persistentObject.SetActive(true);
        
        PlayerPrefs.SetInt("PopupShown", 1);
        PlayerPrefs.Save();
    }

    // 用于开发测试的复位方法
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("FirstLaunch");
        PlayerPrefs.DeleteKey("PopupShown");
    }
}