using UnityEngine;

public class MovementController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // 移动速度
    public bool IsInputEnabled { get; set; } = true; // 是否启用输入

    private Vector2 currentInput;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// 设置移动输入
    /// </summary>
    public void SetMovementInput(Vector2 input)
    {
        currentInput = input;
    }

    private void FixedUpdate()
    {
        if (!IsInputEnabled) return;

        // 实际移动逻辑
        rb.velocity = currentInput * moveSpeed;
    }
}