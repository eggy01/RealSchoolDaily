using UnityEngine;

public class Node : IHeapItem<Node>
{
    public bool walkable;
    public Vector2 worldPosition;
    public int gridX, gridY;
    public int gCost, hCost;
    public Node parent;
    public int HeapIndex { get; set; } // 用于堆排序

    public Node(bool _walkable, Vector2 _worldPos, int _gridX, int _gridY)
    {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        gCost = int.MaxValue;
        hCost = int.MaxValue;
    }

    public int CompareTo(Node other)
    {
        // 正确比较 fCost（gCost + hCost）
        int compare = (gCost + hCost).CompareTo(other.gCost + other.hCost);
        if (compare == 0)
        {
            // 当总成本相同时，比较 hCost
            compare = hCost.CompareTo(other.hCost);
        }
        // 返回负值以实现最小堆
        return -compare;
    }
}