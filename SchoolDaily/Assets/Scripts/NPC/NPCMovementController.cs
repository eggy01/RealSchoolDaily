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

    // 用于存储路径信息
    private float initialPathTotalTime; // 初始路径的总时间
    private DateTime pathStartTime;     // 路径开始时间

    public UnityEvent OnReachedDestination;

    void Awake()
    {
        scheduleData = GetComponent<NPCScheduleData>();
        InitializeTimeManager();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        SubscribeToSceneLoadEvent();
        if (TimeManager.Instance != null)
        {
            CheckSchedule(TimeManager.Instance.GetHour());
        }
    }

    void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnHourChanged -= CheckSchedule;
        }
    }

    public void SubscribeToSceneLoadEvent()
    {
        EventHandler.AfterScenLoadEvent += InitializeNpcPositionOnLoad;
    }

    public void UnsubscribeToSceneLoadEvent()
    {
        EventHandler.AfterScenLoadEvent -= InitializeNpcPositionOnLoad;
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

    private void CheckSchedule(int currentHour)
    {
        var specialEntry = FindMatchingSpecialSchedule();
        if (HandleSpecialSchedule(specialEntry, currentHour)) return;

        foreach (var entry in scheduleData.dailySchedule)
        {
            if (currentHour >= entry.startHour && currentHour < entry.endHour)
            {
                MoveTo(entry.targetPosition);
                return;
            }
        }
        StopMovement();
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

        while (currentPathIndex < currentPath.Count)
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
        // 到达目的地后停止动画
        animator.SetBool(movingAnimParam, false);
        isRecalculating = false;
        FinalizeMovement();
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

        // 计算路径的总时间和初始时间
        float totalDistance = 0f;
        for (int i = 1; i < currentPath.Count; i++)
        {
            totalDistance += Vector2.Distance(currentPath[i - 1], currentPath[i]);
        }
        initialPathTotalTime = (totalDistance / moveSpeed) * (60f / Settings.minuteThreshold);
        pathStartTime = TimeManager.Instance.GetCurrentDate();

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

    // 在场景加载后初始化NPC的位置
    public void InitializeNpcPositionOnLoad()
    {
        if (!currentPath.IsNullOrEmpty())
        {
            DateTime currentTime = TimeManager.Instance.GetCurrentDate();
            TimeSpan timeSincePathStart = currentTime - pathStartTime;

            if (timeSincePathStart.TotalSeconds > 0)
            {
                float completionPercentage = Mathf.Clamp01((float)(timeSincePathStart.TotalSeconds / initialPathTotalTime));
                Vector2 startPosition = FindPositionOnPathAtTime(completionPercentage);
                transform.position = startPosition;
            }
        }
    }

    // 根据完成比例在路径上找到位置
    private Vector2 FindPositionOnPathAtTime(float timePercentage)
    {
        if (currentPath.IsNullOrEmpty()) return transform.position;

        if (timePercentage >= 1f) return currentPath[currentPath.Count - 1];
        if (timePercentage <= 0f) return currentPath[0];

        float remainingTime = initialPathTotalTime * timePercentage;
        float distanceTraveled = moveSpeed * remainingTime * (60f / Settings.minuteThreshold);

        float totalDistance = 0f;
        for (int i = 1; i < currentPath.Count; i++)
        {
            float currentSegmentDistance = Vector2.Distance(currentPath[i - 1], currentPath[i]);
            if (totalDistance + currentSegmentDistance >= distanceTraveled)
            {
                float remainingDistance = distanceTraveled - totalDistance;
                Vector2 direction = (currentPath[i] - currentPath[i - 1]).normalized;
                return currentPath[i - 1] + direction * remainingDistance;
            }
            totalDistance += currentSegmentDistance;
        }

        return currentPath[currentPath.Count - 1];
    }

    // 视觉化调试显示
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

        // 显示NPC的初始位置
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Vector2 startPosition = FindPositionOnPathAtTime(Mathf.Clamp01(initialPathTotalTime));
            Gizmos.DrawSphere(startPosition, 0.5f);
        }
    }
}

public static class PathExtensions
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
    {
        return enumerable == null || !enumerable.GetEnumerator().MoveNext();
    }
}