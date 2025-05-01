using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(NPCScheduleData))]
public class NPCMovementController : MonoBehaviour
{
    #region 配置
    [Header("时间模拟")]
    public bool usePreciseTiming = true;

    [Header("移动设置")]
    public float moveSpeed = 3f;
    public float pathUpdateThreshold = 0.1f;
    public LayerMask obstacleMask;

    [Header("调试")]
    public bool showPathGizmos = true;

    [Header("动画参数")]
    [SerializeField] private string horizontalAnimParam = "horizontal";
    [SerializeField] private string verticalAnimParam = "vertical";
    [SerializeField] private string movingAnimParam = "isWalking";
    #endregion

    #region 私有变量
    // 时间计算相关
    private float initialPathTotalTime;
    private DateTime pathStartTime;

    // 路径相关
    private List<Vector2> currentPath;
    private int currentPathIndex;
    private Vector2 lastTargetPosition;

    // 组件引用
    private NPCScheduleData scheduleData;
    private Animator animator;

    // 状态控制
    private Coroutine moveCoroutine;
    private bool isRecalculating = false;
    private Vector2 lastMoveDirection;
    #endregion

    public UnityEvent OnReachedDestination;

    #region Unity生命周期
    void Awake()
    {
        scheduleData = GetComponent<NPCScheduleData>();
        animator = GetComponent<Animator>();
        InitializeTimeManager();
    }

    void Start()
    {
        if (TimeManager.Instance != null)
        {
            int currentHour = TimeManager.Instance.GetHour();
            CheckSchedule(currentHour);

            if (currentPath == null || currentPath.Count == 0)
            {
                transform.position = FindLastDailyPositionBefore(currentHour);
            }
        }
    }

    void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnHourChanged -= CheckSchedule;
        }
    }

    void OnDrawGizmos()
    {
        if (!showPathGizmos || currentPath == null) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < currentPath.Count; i++)
        {
            Gizmos.DrawSphere(currentPath[0], 1f);
            Gizmos.DrawSphere(currentPath[^1], 1f);
            Gizmos.DrawSphere(currentPath[i], 0.1f);
            if (i > 0) Gizmos.DrawLine(currentPath[i - 1], currentPath[i]);
        }
    }
    #endregion

    #region 核心移动逻辑
    public void MoveTo(Vector2 targetPosition)
    {
        if (ShouldSkipMovement(targetPosition)) return;

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        // 生成新路径时重置时间基准
        if (currentPath == null || currentPath.Count == 0 || pathStartTime == default)
        {
            currentPath = Pathfinding2D.Instance.FindPath(transform.position, targetPosition);
            LogPathDebugInfo();

            if (IsPathInvalid())
            {
                Debug.LogWarning("路径计算失败，目标可能不可达");
                return;
            }

            // 初始化路径时间参数
            initialPathTotalTime = CalculatePathTotalTime(currentPath);
            pathStartTime = TimeManager.Instance.GetCurrentDate();
            Debug.Log($"新路径初始化 | 总时间: {initialPathTotalTime}s 开始时间: {pathStartTime:HH:mm:ss}");
        }

        // 计算已过时间（基于初始路径开始时间）
        TimeSpan elapsedTime = TimeManager.Instance.GetCurrentDate() - pathStartTime;
        float elapsedSeconds = (float)elapsedTime.TotalSeconds;
        float progress = initialPathTotalTime > 0 ? Mathf.Clamp01(elapsedSeconds / initialPathTotalTime) : 0;

        Debug.Log($"移动进度 | 已过时间: {elapsedSeconds:F1}s 进度: {progress:P0}");

        if (progress >= 1f)
        {
            // 到达终点处理
            transform.position = targetPosition;
            currentPath = null;
            initialPathTotalTime = 0;
            pathStartTime = default;
            OnReachedDestination?.Invoke();
            Debug.Log("已到达最终目的地");
            return;
        }
        else if (progress > 0f)
        {
            // 根据进度调整位置
            Vector2 newPos = GetPositionAlongPath(currentPath, progress);
            Debug.Log($"进度调整 | 新位置: {newPos}");

            // 截断路径并更新时间基准
            int startIndex = FindClosestPathIndex(newPos);
            if (startIndex > 0 && startIndex < currentPath.Count)
            {
                currentPath = currentPath.GetRange(startIndex, currentPath.Count - startIndex);

                // 计算剩余时间并调整时间基准
                float remainingTime = initialPathTotalTime * (1 - progress);
                pathStartTime = TimeManager.Instance.GetCurrentDate().AddSeconds(-remainingTime);
                initialPathTotalTime = remainingTime;

                Debug.Log($"路径截断 | 剩余时间: {remainingTime:F1}s 新起点: {pathStartTime:HH:mm:ss}");
            }
        }

        // 验证并开始移动
        ValidatePath(targetPosition);
    }

    private float CalculatePathTotalTime(List<Vector2> path)
    {
        if (path == null || path.Count < 2) return 0f;
        //计算总距离
        float totalDistance = 0f;
        for (int i = 1; i < path.Count; i++)
            totalDistance += Vector2.Distance(path[i - 1], path[i]);

        // 现实时间
        float realTimeTotalTime = totalDistance / moveSpeed;
        Debug.Log("realTimeTotalTime" + realTimeTotalTime);
        // 游戏内时间
        float gameTimeTotalTime = realTimeTotalTime * (60f / Settings.minuteThreshold);
        Debug.Log("gameTimeTotalTime" + gameTimeTotalTime);
        //时间=路程/速度
        return gameTimeTotalTime;

    }

    private Vector2 GetPositionAlongPath(List<Vector2> path, float progress)
    {
        float totalDistance = initialPathTotalTime * moveSpeed;
        float targetDistance = totalDistance * progress;

        float accumulated = 0f;
        for (int i = 1; i < path.Count; i++)
        {
            float segment = Vector2.Distance(path[i - 1], path[i]);
            if (accumulated + segment >= targetDistance)
            {
                float t = (targetDistance - accumulated) / segment;
                return Vector2.Lerp(path[i - 1], path[i], t);
            }
            accumulated += segment;
        }
        return path[^1];
    }

    private int FindClosestPathIndex(Vector2 position)
    {
        float minDist = float.MaxValue;
        int closestIndex = 0;

        for (int i = 0; i < currentPath.Count; i++)
        {
            float dist = Vector2.Distance(position, currentPath[i]);
            if (dist < minDist)
            {
                minDist = dist;
                closestIndex = i;
            }
        }
        return closestIndex;
    }

    private IEnumerator FollowPath()
    {
        InitializePathFollowing();

        while (currentPathIndex < currentPath.Count)
        {
            Vector2 targetPoint = currentPath[currentPathIndex];
            UpdateMovement(targetPoint);
            HandlePathProgress(targetPoint);
            yield return null;
        }

        FinalizeMovement();
    }

    private void UpdateAnimationParameters(Vector2 moveDirection)
    {
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

        animator.SetBool(movingAnimParam, moveDirection.magnitude > 0.1f);
        if (moveDirection.magnitude > 0.1f) lastMoveDirection = moveDirection;
    }
    #endregion

    #region 日程安排处理
    private void CheckSchedule(int currentHour)
    {
        var specialEntry = FindMatchingSpecialSchedule();
        if (HandleSpecialSchedule(specialEntry, currentHour)) return;

        HandleDailySchedule(currentHour);
    }

    private void HandleDailySchedule(int currentHour)
    {
        for (int i = 0; i < scheduleData.dailySchedule.Length; i++)
        {
            var entry = scheduleData.dailySchedule[i];
            if (!IsWithinScheduleTime(currentHour, entry)) continue;

            Vector2 startPos = GetScheduleStartPosition(i);
            InitializePositionIfNeeded(startPos);

            // 如果已经在目标点则跳过
            if (Vector2.Distance(transform.position, entry.targetPosition) < 0.1f)
                return;

            MoveTo(entry.targetPosition);
            return;
        }
        StopMovement();
    }

    private bool HandleSpecialSchedule(SpecialScheduleEntry entry, int currentHour)
    {
        if (entry == null) return false;

        if (currentHour >= entry.startHour && currentHour < entry.endHour)
        {
            Vector2 startPos = FindLastDailyPositionBefore(currentHour);
            InitializePositionIfNeeded(startPos);
            MoveTo(entry.targetPosition);
            return true;
        }
        return false;
    }
    #endregion

    #region 辅助方法
    private void InitializeTimeManager()
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

    private Vector2 FindLastDailyPositionBefore(int currentHour)
    {
        Vector2 lastPos = transform.position;
        foreach (var entry in scheduleData.dailySchedule)
        {
            if (entry.endHour <= currentHour) lastPos = entry.targetPosition;
        }
        return lastPos;
    }

    private SpecialScheduleEntry FindMatchingSpecialSchedule()
    {
        if (TimeManager.Instance == null) return null;
        return Array.Find(scheduleData.specialSchedule, entry =>
            entry.month == TimeManager.Instance.GetMonth() &&
            entry.day == TimeManager.Instance.GetDay());
    }

    private void StopMovement()
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        currentPath = null;
        moveCoroutine = null;
    }
    #endregion

    #region 移动辅助
    private bool ShouldSkipMovement(Vector2 targetPosition)
    {
        return Pathfinding2D.Instance == null ||
               Vector2.Distance(transform.position, targetPosition) < 0.1f ||
               Vector2.Distance(lastTargetPosition, targetPosition) < 0.1f;
    }

    private void LogPathDebugInfo()
    {
        if (currentPath != null && currentPath.Count > 0)
        {
            Debug.Log($"路径起点: {currentPath[0]}");
            Debug.Log($"路径终点: {currentPath[^1]}");
        }
    }

    private bool IsPathInvalid()
    {
        return currentPath == null || currentPath.Count == 0;
    }

    private void ValidatePath(Vector2 targetPosition)
    {
        if (currentPath == null || currentPath.Count == 0) return;
        moveCoroutine = StartCoroutine(FollowPath());
    }

    private void InitializePathFollowing()
    {
        currentPathIndex = 0;
        lastTargetPosition = currentPath[^1];
        isRecalculating = false;
    }

    private void UpdateMovement(Vector2 targetPoint)
    {
        Vector2 moveDirection = (targetPoint - (Vector2)transform.position).normalized;
        UpdateAnimationParameters(moveDirection);
        transform.position = Vector2.MoveTowards(transform.position, targetPoint, moveSpeed * Time.deltaTime);
    }

    private void HandlePathProgress(Vector2 targetPoint)
    {
        if (Physics2D.Linecast(transform.position, targetPoint, obstacleMask))
        {
            isRecalculating = true;
            MoveTo(lastTargetPosition);
        }

        if (Vector2.Distance(transform.position, targetPoint) <= pathUpdateThreshold)
        {
            currentPathIndex++;
        }
    }

    private void FinalizeMovement()
    {
        animator.SetBool(movingAnimParam, false);
        animator.SetFloat(horizontalAnimParam, Mathf.Sign(lastMoveDirection.x));
        animator.SetFloat(verticalAnimParam, Mathf.Sign(lastMoveDirection.y));
        OnReachedDestination?.Invoke();
    }
    #endregion

    #region 日程辅助
    private Vector2 GetScheduleStartPosition(int index)
    {
        return index > 0 ?
            scheduleData.dailySchedule[index - 1].targetPosition :
            transform.position;
    }

    private void InitializePositionIfNeeded(Vector2 position)
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            transform.position = position;
        }
    }

    private bool IsWithinScheduleTime(int currentHour, ScheduleEntry entry)
    {
        return currentHour >= entry.startHour && currentHour < entry.endHour;
    }
    #endregion
}