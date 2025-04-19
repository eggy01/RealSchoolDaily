using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneToInitialPosition : MonoBehaviour
{
    // 字典，存储场景名称和对应的初始位置
    private Dictionary<string, Vector3> vector3Dictionary;

    // 单例实例
    public static SceneToInitialPosition Instance;


    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 初始化字典
        vector3Dictionary = new Dictionary<string, Vector3>();

        // 添加一些键值对到字典中
        vector3Dictionary.Add("Life Scene", new Vector3(27f, 38f, 0f));
    }

    // 外界接口，获取特定场景的初始位置
    public Vector3 GetInitialPosition(string sceneName)
    {
        if (vector3Dictionary.TryGetValue(sceneName, out Vector3 position))
        {
            return position;
        }
        else
        {
            Debug.LogWarning("Scene '" + sceneName + "' not found in dictionary.");
            return Vector3.zero; // 返回默认值
        }
    }
}
