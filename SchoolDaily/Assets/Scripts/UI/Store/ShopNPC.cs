using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopNPC : MonoBehaviour
{
    public string shopType; // 在Inspector中设置商店类型（如"小卖部"）
    private bool isInRange;  // 用于检测玩家是否在触发范围内

    private void Update()
    {
        // 当玩家在范围内且按下E键时
        if ( Input.GetKeyDown(KeyCode.T))
        {
            ShopUI.Instance.ShowShop(shopType);
            Debug.Log("T");
        }
    }

    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other.CompareTag("Player"))
    //     {
    //         isInRange = true;
    //         // 可以在这里添加UI提示（如显示"按E交易"）
    //     }
    // }

    // private void OnTriggerExit(Collider other)
    // {
    //     if (other.CompareTag("Player"))
    //     {
    //         isInRange = false;
    //         // 可以在这里关闭UI提示
    //         //ShopUI.Instance.HideShop(); // 可选：离开时自动关闭商店
    //     }
    // }
}