using UnityEngine;

public class InventoryUIHandler : MonoBehaviour
{
    public static InventoryUIHandler Instance;
    public GameObject myBag;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        myBag.SetActive(false);
    }

    public void ToggleInventory()
    {
        bool newState = !myBag.activeSelf;

        if (newState)
        {
            CloseAllDetails();
            BagUI.Instance.ForceRefresh();

            // 关闭商店
            if (ShopUI.Instance != null)
            {
                ShopUI.Instance.CloseShop();
            }
        }

        myBag.SetActive(newState);
        UpdatePauseState();
    }

    public void CloseInventory()
    {
        myBag.SetActive(false);
        CloseAllDetails();

        // 如果商店没有打开，取消暂停
        if (ShopUI.Instance != null && !ShopUI.Instance.shopPanel.activeSelf)
        {
            PauseManager.Instance.SetPauseState(false);
        }
    }

    public void CloseAllDetails()
    {
        BagUI.Instance?.ForceRefresh();
    }

    private void UpdatePauseState()
{
    bool shouldPause = myBag.activeSelf || (ShopUI.Instance != null && ShopUI.Instance.shopPanel.activeSelf);
    PauseManager.Instance.SetPauseState(shouldPause);
    PlayerController.Instance.movement.SetPause(shouldPause);
}
}