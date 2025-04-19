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

    private NPCScheduleData scheduleData;
    private List<Vector2> currentPath;
    private int currentPathIndex;
    private bool isMoving = false;
    private Coroutine moveCoroutine;

    // 事件：到达目标位置
    public UnityEvent OnReachedDestination;

    void Awake()
    {
        scheduleData = GetComponent<NPCScheduleData>();
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
        // 检查 TimeManager 是否已初始化
        if (TimeManager.Instance != null)
        {
            CheckSchedule(TimeManager.Instance.GetHour());
        }
    }

    private void CheckSchedule(int currentHour)
    {
        // 优先检查特殊日程
        SpecialScheduleEntry specialEntry = FindMatchingSpecialSchedule();
        if (specialEntry != null)
        {
            if (currentHour >= specialEntry.startHour && currentHour < specialEntry.endHour)
            {
                MoveTo(specialEntry.targetPosition);
                return;
            }
        }

        // 检查日常行程
        foreach (ScheduleEntry entry in scheduleData.dailySchedule)
        {
            if (currentHour >= entry.startHour && currentHour < entry.endHour)
            {
                MoveTo(entry.targetPosition);
                return;
            }
        }

        // 没有匹配行程时停止移动
        StopMovement();
    }

    private SpecialScheduleEntry FindMatchingSpecialSchedule()
    {
        if (TimeManager.Instance == null) return null;

        int currentDay = TimeManager.Instance.GetDay();
        int currentMonth = TimeManager.Instance.GetMonth();

        foreach (SpecialScheduleEntry entry in scheduleData.specialSchedule)
        {
            if (entry.month == currentMonth && entry.day == currentDay)
            {
                return entry;
            }
        }
        return null;
    }

    public void MoveTo(Vector2 targetPosition)
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        currentPath = Pathfinding2D.Instance.FindPath(transform.position, targetPosition);
        
        if (currentPath == null || currentPath.Count == 0)
        {
            Debug.LogWarning("无法找到有效路径！");
            return;
        }

        moveCoroutine = StartCoroutine(FollowPath());
    }

    private IEnumerator FollowPath()
    {
        isMoving = true;
        currentPathIndex = 0;

        while (currentPathIndex < currentPath.Count)
        {
            Vector2 targetPoint = currentPath[currentPathIndex];
            
            transform.position = Vector2.MoveTowards(
                transform.position, 
                targetPoint, 
                moveSpeed * Time.deltaTime
            );

            // 精确坐标判断
            if ((Vector2)transform.position == targetPoint)
            {
                currentPathIndex++;
            }

            yield return null;
        }

        isMoving = false;
        OnReachedDestination?.Invoke();
    }

    private void StopMovement()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            isMoving = false;
        }
    }

    void OnDrawGizmos()
    {
        if (!showPathGizmos || currentPath == null) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < currentPath.Count; i++)
        {
            Gizmos.DrawSphere(currentPath[i], 0.1f);
            if (i > 0)
                Gizmos.DrawLine(currentPath[i - 1], currentPath[i]);
        }
    }
}