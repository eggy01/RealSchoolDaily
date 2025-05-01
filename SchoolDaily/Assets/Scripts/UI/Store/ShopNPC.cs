using UnityEngine;

public class ShopNPC : MonoBehaviour
{
    public string shopType; // 在Inspector中设置商店类型（如"小卖部"）
    private bool isInRange;  // 用于检测玩家是否在触发范围内
    public GameObject talkUI;

    private void Update()
    {
        // 当玩家在范围内且按下E键时
        if (isInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (ShopUI.Instance.shopPanel.activeSelf)
            {
                ShopUI.Instance.CloseShop();
            }
            else
            {
                ShopUI.Instance.ShowShop(shopType);
            }
        }

        // 当按下ESC键时关闭商店界面
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (ShopUI.Instance.shopPanel.activeSelf)
            {
                ShopUI.Instance.CloseShop();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("玩家进入超市触发器");
            isInRange = true;
            talkUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("玩家离开超市触发器");
            isInRange = false;
            talkUI.SetActive(false);
        }
    }
}