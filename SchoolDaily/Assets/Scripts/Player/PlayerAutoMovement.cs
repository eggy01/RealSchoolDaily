using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAutoMovement : MonoBehaviour
{
    // 移动配置
    [Header("移动设置")]
    public float moveSpeed = 3f;
    public float stopDistance = 0.1f;
    public LayerMask obstacleMask;

    // 自动关联的组件
    private Rigidbody2D rb;
    private Animator animator;
    private MovementController movementController;
    private bool isAutoMoving = false;
    [Header("动画参数")]
    [SerializeField] private string horizontalAnimParam = "horizontal";
    [SerializeField] private string verticalAnimParam = "vertical";
    [SerializeField] private string movingAnimParam = "isWalking";
    private Vector2 lastMoveDirection;




    // 事件系统
    public UnityEvent OnMovementStart;
    public UnityEvent OnMovementComplete;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        movementController = GetComponent<MovementController>();

        if (movementController == null)
            movementController = gameObject.AddComponent<MovementController>();
    }

    /// <summary>
    /// 外部调用接口（静态方法）
    /// </summary>
    public static void MoveToPosition(Vector2 targetPosition)
    {
        var player = FindPlayer();
        if (player != null)
        {
            player.GetComponent<PlayerAutoMovement>()._StartAutoMove(targetPosition);
        }
    }

    // 实际移动协程
    private IEnumerator AutoMoveRoutine(Vector2 targetPosition)
    {
        isAutoMoving = true;
        if (movementController) movementController.IsInputEnabled = false;

        while (Vector2.Distance(transform.position, targetPosition) > stopDistance)
        {
            Vector2 dir = (targetPosition - (Vector2)transform.position).normalized;
            rb.velocity = dir * moveSpeed;

            // 更新动画参数
            UpdateAnimationParameters(dir);

            if (Physics2D.Linecast(transform.position, targetPosition, obstacleMask))
                break;

            yield return null;
        }

        rb.velocity = Vector2.zero;
        UpdateAnimationParameters(Vector2.zero); // 停止动画
        isAutoMoving = false;
        if (movementController) movementController.IsInputEnabled = true;
    }

    private void UpdateAnimationParameters(Vector2 moveDirection)
    {
        if (animator == null) return;

        // 优先使用较大绝对值的方向
        if (Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.y))
        {
            animator.SetFloat(horizontalAnimParam, Mathf.Sign(moveDirection.x));
            animator.SetFloat(verticalAnimParam, 0);
        }
        else
        {
            animator.SetFloat(verticalAnimParam, Mathf.Sign(moveDirection.y));
            animator.SetFloat(horizontalAnimParam, 0);
        }

        // 更新移动状态
        animator.SetBool(movingAnimParam, moveDirection.magnitude > 0.1f);

        // 记录最后移动方向（用于Idle状态）
        if (moveDirection.magnitude > 0.1f)
        {
            lastMoveDirection = moveDirection;
        }
    }

    // 私有启动方法
    private void _StartAutoMove(Vector2 targetPosition)
    {
        if (!isAutoMoving)
        {
            StartCoroutine(AutoMoveRoutine(targetPosition));
        }
    }

    // 静态方法查找玩家
    public static GameObject FindPlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("未找到带有Player标签的对象");
        }
        return player;
    }

    public bool IsMoving() => isAutoMoving;
}