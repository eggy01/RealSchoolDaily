using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerAutoMovement : MonoBehaviour
{
    public static PlayerAutoMovement Instance { get; private set; }
    // 移动配置
    [Header("移动设置")]
    public float moveSpeed = 3f; // 移动速度
    public float stopDistance = 0.1f; // 停止距离

    // 自动关联的组件
    private Rigidbody2D rb;
    private Animator animator;

    // 是否正在自动移动（用于外部判断）
    public bool isAutoMoving { get; private set; } = false;

    // 动画参数名称
    [Header("动画参数")]
    public string horizontalAnimParam = "horizontal";
    public string verticalAnimParam = "vertical";
    public string movingAnimParam = "isWalking";

    void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// 外部调用接口
    /// </summary>
    public void MoveToPosition(Vector2 targetPosition)
    {
        if (!isAutoMoving)
        {
            StartCoroutine(AutoMoveRoutine(targetPosition));
        }
    }

    // 实际移动协程
    private IEnumerator AutoMoveRoutine(Vector2 targetPosition)
    {
        isAutoMoving = true;
        rb.gravityScale = 0; // 禁用重力
        rb.drag = 0; // 禁用阻力

        while (Vector2.Distance(transform.position, targetPosition) > 0.1f)
        {
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            //rb.velocity = direction * moveSpeed;
            UpdateAnimationParameters(direction);
            transform.Translate(direction * moveSpeed * Time.deltaTime);

            yield return null;
        }

        rb.velocity = Vector2.zero;
        UpdateAnimationParameters(Vector2.zero);
        isAutoMoving = false;
        Debug.Log("自动移动完成");
    }

    private void UpdateAnimationParameters(Vector2 direction)
    {
        animator.SetFloat(horizontalAnimParam, direction.x);
        animator.SetFloat(verticalAnimParam, direction.y);
        animator.SetBool(movingAnimParam, direction.magnitude > 0.01f);
    }
}