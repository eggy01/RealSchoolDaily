using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DormMapManager : MonoBehaviour
{
    public GameObject gameObject0;
    public GameObject gameObject1;
    void Awake()
    {
        Debug.Log("dormmap醒");
        if (!StoryProgressManager.Instance.IsStoryCompleted("Beginner_01"))
        {
            gameObject0.SetActive(true);
            gameObject1.SetActive(false);
            Debug.Log("1111");
        }
    }
    void Update()
    {
        if (StoryProgressManager.Instance.IsStoryCompleted("Beginner_01"))
        {
            Debug.Log("销毁");
            gameObject0.SetActive(false);
            gameObject1.SetActive(true);
            Destroy(gameObject0);
            Destroy(this);
        }
    }
}
