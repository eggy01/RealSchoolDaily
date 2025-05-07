// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class BusRoute : MonoBehaviour
// {
//     public List<Transform> waypoints; // 路线点
//     public List<BusStop> busStops; // 公交站列表
//     public float speed = 5f;
//     public float stopDuration = 10f;

//     private int currentWaypoint = 0;
//     private bool isStopped = false;
//     private float stopTimer = 0f;
//     void Awake()
//     {
//         DontDestroyOnLoad(gameObject);
//     }
//     void Update()
//     {
//         if (isStopped)
//         {
//             stopTimer += Time.deltaTime;
//             if (stopTimer >= stopDuration)
//             {
//                 isStopped = false;
//                 currentWaypoint++;
//                 if (currentWaypoint >= waypoints.Count)
//                 {
//                     currentWaypoint = 0; // 循环路线
//                 }
//             }
//             return;
//         }

//         Transform target = waypoints[currentWaypoint];
//         transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

//         if (Vector3.Distance(transform.position, target.position) < 0.1f)
//         {
//             // 检查是否是公交站
//             BusStop stop = busStops.Find(s => s.waypointIndex == currentWaypoint);
//             if (stop != null)
//             {
//                 isStopped = true;
//                 stopTimer = 0f;
//                 stop.OnBusArrived(this); // 通知站牌公交车已到达
//             }
//             else
//             {
//                 currentWaypoint++;
//                 if (currentWaypoint >= waypoints.Count)
//                 {
//                     currentWaypoint = 0; // 循环路线
//                 }
//             }
//         }
//     }
// }
