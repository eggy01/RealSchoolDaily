using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    public bool IsInputEnabled { get; set; } = true;

    private Vector2 currentInput;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// 设置移动输入（供PlayerAutoMovement调用）
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