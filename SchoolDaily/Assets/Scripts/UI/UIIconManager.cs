using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIIconManager : MonoBehaviour
{
    public Button forum;
    void Start()
    {
        forum.onClick.AddListener(() => WindowManager.Instance.OpenWindow(ForumUIManager.Instance));
    }
}
