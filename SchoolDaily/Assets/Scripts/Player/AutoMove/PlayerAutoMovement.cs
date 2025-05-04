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
        Debug.Log($"开始自动移动到: {targetPosition}");

        // 禁用物理自动减速
        rb.drag = 0;

        while (Vector2.Distance(transform.position, targetPosition) > stopDistance)
        {
            Vector2 currentPos = transform.position;
            Vector2 rawDirection = targetPosition - currentPos;
            Vector2 direction = rawDirection.normalized;

            // 直接修改位置而不是使用速度（测试用）
            // transform.position = Vector2.MoveTowards(currentPos, targetPosition, moveSpeed * Time.deltaTime);

            // 使用速度移动
            rb.velocity = direction * moveSpeed;

            UpdateAnimationParameters(direction);

            // 可视化调试线
            Debug.DrawLine(currentPos, targetPosition, Color.red);

            // 障碍物检测（更精确的检测方式）
            RaycastHit2D hit = Physics2D.Raycast(currentPos, direction, rawDirection.magnitude, obstacleMask);
            if (hit.collider != null)
            {
                Debug.Log($"检测到障碍物: {hit.collider.name}", hit.collider.gameObject);
                break;
            }

            yield return null;
        }

        rb.velocity = Vector2.zero;
        UpdateAnimationParameters(Vector2.zero);
        isAutoMoving = false;
        Debug.Log("自动移动完成");
    }

    private void UpdateAnimationParameters(Vector2 direction)
    {
        // 调试输出原始方向值
        Debug.Log($"原始移动方向: {direction}");

        // 标准化方向向量
        direction = direction.normalized;

        // 调试输出标准化后的方向
        Debug.Log($"标准化后方向: {direction}");

        // 确保参数名正确（注意大小写）
        animator.SetFloat(horizontalAnimParam, direction.x);
        animator.SetFloat(verticalAnimParam, direction.y);

        // 设置移动状态（使用更小的阈值）
        bool isMoving = direction.sqrMagnitude > 0.01f;
        animator.SetBool(movingAnimParam, isMoving);

        // 调试输出动画参数值
        Debug.Log($"动画参数 - 水平: {direction.x}, 垂直: {direction.y}, 移动中: {isMoving}");
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