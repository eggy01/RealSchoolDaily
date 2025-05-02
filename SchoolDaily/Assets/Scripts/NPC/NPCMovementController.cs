using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(NPCScheduleData))]
public class NPCMovementController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 3f;
    public float pathUpdateThreshold = 0.1f;
    public LayerMask obstacleMask;

    [Header("调试")]
    public bool showPathGizmos = true;
    private bool isRecalculating = false;

    [Header("动画参数")]
    [SerializeField] private string horizontalAnimParam = "horizontal";
    [SerializeField] private string verticalAnimParam = "vertical";
    [SerializeField] private string movingAnimParam = "isWalking";
    private Animator animator;
    private Vector2 lastMoveDirection;

    private NPCScheduleData scheduleData;
    private List<Vector2> currentPath;
    private int currentPathIndex;
    private Coroutine moveCoroutine;
    private Vector2 lastTargetPosition;

    private ScheduleEntry currentDailyEntry; // 当前日常日程条目
    private SpecialScheduleEntry currentSpecialEntry; // 当前特殊日程条目
    private bool useSpecialSchedule; // 是否使用特殊日程

    public UnityEvent OnReachedDestination;


    void Awake()
    {
        scheduleData = GetComponent<NPCScheduleData>();
        InitializeTimeManager();
        animator = GetComponent<Animator>();
    }

    void InitializeTimeManager()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnHourChanged += CheckSchedule;
        }
        else
        {
            Debug.LogError("TimeManager instance not found!");
        }
    }

    void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnHourChanged -= CheckSchedule;
        }
    }

    void Start()
    {
        scheduleData = GetComponent<NPCScheduleData>();
        animator = GetComponent<Animator>();

        if (TimeManager.Instance != null)
        {
            CalculateInitialPosition();
            CheckSchedule(TimeManager.Instance.GetHour());
        }
    }

    private void CheckSchedule(int currentHour)
    {
        // 强制传送检查
        if (useSpecialSchedule)
        {
            if (currentHour >= currentSpecialEntry.endHour)
                ForceFinalizeMovement(currentSpecialEntry.targetPosition);
        }
        else if (currentDailyEntry != null)
        {
            if (currentHour >= currentDailyEntry.endHour)
                ForceFinalizeMovement(currentDailyEntry.targetPosition);
        }

        // 正常日程检查
        var specialEntry = FindMatchingSpecialSchedule();
        if (HandleSpecialSchedule(specialEntry, currentHour)) return;

        bool isInSchedule = false;
        foreach (var entry in scheduleData.dailySchedule)
        {
            if (currentHour >= entry.startHour && currentHour < entry.endHour)
            {
                isInSchedule = true;
                if (currentDailyEntry == null || entry.startHour != currentDailyEntry.startHour)
                {
                    StopMovement();
                    transform.position = entry.startPosition;
                    MoveTo(entry.targetPosition);
                    currentDailyEntry = entry;
                    useSpecialSchedule = false;
                }
                return;
            }
        }

        // 如果不在任何日程时间段内
        if (!isInSchedule)
        {
            StopMovement();
            // 销毁NPC对象
            Destroy(gameObject);
            return;
        }
        StopMovement();
    }
    private void ForceFinalizeMovement(Vector2 targetPosition)
    {
        StopMovement();
        transform.position = targetPosition;
        animator.SetBool(movingAnimParam, false);
        OnReachedDestination?.Invoke();
    }

    private bool HandleSpecialSchedule(SpecialScheduleEntry entry, int currentHour)
    {
        if (entry != null && currentHour >= entry.startHour && currentHour < entry.endHour)
        {
            MoveTo(entry.targetPosition);
            return true;
        }
        return false;
    }

    private SpecialScheduleEntry FindMatchingSpecialSchedule()
    {
        if (TimeManager.Instance == null) return null;

        return Array.Find(scheduleData.specialSchedule, entry =>
            entry.month == TimeManager.Instance.GetMonth() &&
            entry.day == TimeManager.Instance.GetDay());
    }

    private IEnumerator FollowPath()
    {
        currentPathIndex = 0;
        lastTargetPosition = currentPath[^1];

        float startTime = Time.time;
        float maxDuration = CalculateMaxDuration();

        while (currentPathIndex < currentPath.Count && (Time.time - startTime) < maxDuration)
        {
            Vector2 targetPoint = currentPath[currentPathIndex];

            // 计算移动方向
            Vector2 moveDirection = (targetPoint - (Vector2)transform.position).normalized;

            // 更新动画参数
            UpdateAnimationParameters(moveDirection);
            // 使用更平滑的移动方式
            transform.position = Vector2.MoveTowards(
                transform.position,
                targetPoint,
                moveSpeed * Time.deltaTime
            );

            HandlePathObstacleCheck(targetPoint);
            HandleWaypointReached(targetPoint);

            yield return null;
        }

        if ((Time.time - startTime) >= maxDuration)
        {
            transform.position = currentPath[^1];
            FinalizeMovement();
        }
        // 到达目的地后停止动画
        animator.SetBool(movingAnimParam, false);
        isRecalculating = false;
        FinalizeMovement();
    }
    private float CalculateMaxDuration()
    {
        if (useSpecialSchedule)
            return (currentSpecialEntry.endHour - currentSpecialEntry.startHour) * 3600f;

        if (currentDailyEntry != null)
            return (currentDailyEntry.endHour - currentDailyEntry.startHour) * 3600f;

        return float.MaxValue;
    }

    private void UpdateAnimationParameters(Vector2 moveDirection)
    {
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

    private void FinalizeMovement()
    {
        // 保持最后移动方向
        animator.SetFloat(horizontalAnimParam, Mathf.Sign(lastMoveDirection.x));
        animator.SetFloat(verticalAnimParam, Mathf.Sign(lastMoveDirection.y));
        OnReachedDestination?.Invoke();
        moveCoroutine = null;
    }

    private void HandlePathObstacleCheck(Vector2 targetPoint)
    {
        if (isRecalculating) return; // 防止重复调用

        if (Physics2D.Linecast(transform.position, targetPoint, obstacleMask))
        {
            isRecalculating = true; // 设置标志位
            MoveTo(lastTargetPosition);
        }
    }

    private void HandleWaypointReached(Vector2 targetPoint)
    {
        if (Vector2.Distance(transform.position, targetPoint) <= pathUpdateThreshold)
        {
            currentPathIndex++;
        }
    }

    public void MoveTo(Vector2 targetPosition)
    {
        if (Pathfinding2D.Instance == null ||
            Vector2.Distance(transform.position, targetPosition) < 0.1f) return;
        if (Vector2.Distance(lastTargetPosition, targetPosition) < 0.1f) return;

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        currentPath = Pathfinding2D.Instance.FindPath(transform.position, targetPosition);
        if (currentPath != null)
        {
            Debug.Log("起点: " + currentPath[0]);
            Debug.Log("终点: " + currentPath[currentPath.Count - 1]);
        }
        if (currentPath == null || currentPath.Count == 0)
        {
            Debug.LogWarning("路径计算失败，目标可能不可达");
            return;
        }
        ValidatePath(targetPosition);
    }

    private void ValidatePath(Vector2 targetPosition)
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            Debug.LogWarning($"无法到达 {targetPosition}：路径不可达");
            return;
        }

        moveCoroutine = StartCoroutine(FollowPath());
    }

    private void StopMovement()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }
        currentPath = null;
    }

    private void CalculateInitialPosition()
    {
        float currentTime = TimeManager.Instance.GetCurrentTimeInHours();

        // 检查特殊日程
        var specialEntry = FindMatchingSpecialSchedule();
        if (specialEntry != null && currentTime >= specialEntry.startHour && currentTime < specialEntry.endHour)
        {
            HandleSpecialInitialPosition(specialEntry, currentTime);
            useSpecialSchedule = true;
            return;
        }

        // 检查日常日程
        foreach (var entry in scheduleData.dailySchedule)
        {
            if (currentTime >= entry.startHour && currentTime < entry.endHour)
            {
                HandleDailyInitialPosition(entry, currentTime);
                useSpecialSchedule = false;
                return;
            }
        }
    }

    private void HandleSpecialInitialPosition(SpecialScheduleEntry entry, float currentTime)
    {
        float elapsedTime = (currentTime - entry.startHour) * 3600f;
        CalculateAndSetPosition(entry.startPosition, entry.targetPosition, elapsedTime);
        currentSpecialEntry = entry;
    }

    private void HandleDailyInitialPosition(ScheduleEntry entry, float currentTime)
    {
        float elapsedTime = (currentTime - entry.startHour) * 3600f;
        CalculateAndSetPosition(entry.startPosition, entry.targetPosition, elapsedTime);
        currentDailyEntry = entry;
    }

    private void CalculateAndSetPosition(Vector2 startPos, Vector2 targetPos, float elapsedSeconds)
    {
        // 生成原始路径
        List<Vector2> path = Pathfinding2D.Instance.FindPath(startPos, targetPos);
        if (path == null || path.Count == 0) return;

        // 计算应移动距离
        float distanceToMove = moveSpeed * elapsedSeconds;
        Vector2 initialPos = GetPositionAlongPath(path, distanceToMove);

        // 设置初始位置并重新生成路径
        transform.position = initialPos;
        MoveTo(targetPos);
    }

    private Vector2 GetPositionAlongPath(List<Vector2> path, float distance)
    {
        float remaining = distance;
        for (int i = 1; i < path.Count; i++)
        {
            float segmentLength = Vector2.Distance(path[i - 1], path[i]);
            if (remaining <= segmentLength)
            {
                Vector2 dir = (path[i] - path[i - 1]).normalized;
                return path[i - 1] + dir * remaining;
            }
            remaining -= segmentLength;
        }
        return path[path.Count - 1];
    }

    void OnDrawGizmos()
    {
        if (!showPathGizmos || currentPath == null) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < currentPath.Count; i++)
        {
            Gizmos.DrawSphere(currentPath[0], 1f);
            Gizmos.DrawSphere(currentPath[currentPath.Count - 1], 1f);
            Gizmos.DrawSphere(currentPath[i], 0.1f);
            if (i > 0)
                Gizmos.DrawLine(currentPath[i - 1], currentPath[i]);
        }
    }
}