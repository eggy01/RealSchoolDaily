using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SceneLightAnchor : MonoBehaviour
{
    private void Awake()
    {
        if (SceneLightManager.Instance != null)
        {
            // 自动注册到管理器
            var setup = SceneLightManager.Instance.sceneLightSetups
                .Find(x => x.sceneName == gameObject.scene.name);

            if (setup != null) setup.lightsParent = transform;
        }
    }
}
