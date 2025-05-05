using UnityEditor.PackageManager.UI;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    public PlayerMovement movement;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (!movement.IsPaused)
        {
            movement.HandleMovement();
        }
        HandleInventoryInput();
    }

    #region 打开背包
    private void HandleInventoryInput()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (WindowManager.Instance.IsWindowOpen(typeof(BagUI)))
            {
                WindowManager.Instance.CloseWindow(BagUI.Instance);
            }
            else
            {
                WindowManager.Instance.OpenWindow(BagUI.Instance);
            }
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            if (WindowManager.Instance.IsWindowOpen(typeof(ChatSystem)))
            {
                WindowManager.Instance.CloseWindow(ChatSystem.Instance);
            }
            else
            {
                WindowManager.Instance.OpenWindow(ChatSystem.Instance, true);
            }
        }
    }
    #endregion
}