// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class BusStop : MonoBehaviour
// {
//     public int waypointIndex; // 对应的路线点索引
//     public string busRouteName; // 对应的公交线路

//     public void OnBusArrived(BusRoute bus)
//     {
//         // 显示UI提示，玩家可以上车
//         // 例如显示"按E键上车"提示
//     }

//     private void OnTriggerStay(Collider other)
//     {
//         if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
//         {
//             // 玩家上车逻辑
//             BoardBus();
//         }
//     }

//     void BoardBus()
//     {
//         // 将玩家设置为公交车的子物体或调整位置

//     }
// }
