using System.Collections.Generic;
using UnityEngine;

public class ShopNPC : MonoBehaviour
{
    [Header("商店配置")]
    public string shopType = "超市";
    public List<ItemData> customItems; // 可覆盖默认商品
    private bool isInRange;
    public GameObject talkUI;

    private void Update()
    {
        if (isInRange && Input.GetKeyDown(KeyCode.E))
        {
            ToggleShopWindow();
        }
    }
    private void ToggleShopWindow()
    {
        if (WindowManager.Instance.IsWindowOpen(typeof(ShopUI)))
        {
            WindowManager.Instance.CloseWindow(ShopUI.Instance);
        }
        else
        {
            WindowManager.Instance.OpenWindow(ShopUI.Instance, shopType);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = true;
            talkUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = false;
            talkUI.SetActive(false);
            
            // 离开范围时自动关闭商店
            if (WindowManager.Instance.IsWindowOpen(typeof(ShopUI)))
            {
                WindowManager.Instance.CloseWindow(ShopUI.Instance);
            }
        }
    }
}