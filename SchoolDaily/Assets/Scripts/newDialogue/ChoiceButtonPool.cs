using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 选项按钮对象池
public class ChoiceButtonPool : MonoBehaviour
{
    public GameObject buttonPrefab;
    private Stack<GameObject> pool = new Stack<GameObject>();

    public GameObject GetButton()
    {
        if (pool.Count > 0)
        {
            return pool.Pop();
        }
        return Instantiate(buttonPrefab);
    }

    public void ReturnButton(GameObject button)
    {
        button.SetActive(false);
        pool.Push(button);
    }
}
