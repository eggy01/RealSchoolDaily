using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveGame : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        StoryProgressManager.Instance.DeleteSaveFile();
        //StoryProgressManager.Instance.LoadProgress();
    }

    void OnApplicationQuit()
    {
        StoryProgressManager.Instance.SaveProgress();
    }
    // 新增的公共删档方法
    public void DeleteAllSaveData()
    {
        StoryProgressManager.Instance.DeleteSaveFile();

        // 重置内存中的进度数据
        StoryProgressManager.Instance.InitializeStoryProgress();

        Debug.Log("所有存档数据已删除并重置");
    }
}
