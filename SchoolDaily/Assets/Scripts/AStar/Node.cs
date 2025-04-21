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
        // 计算 fCost 比较值（升序）
        int compare = (gCost + hCost).CompareTo(other.gCost + other.hCost);

        // fCost 相等时，用 hCost 作为次优条件（升序）
        if (compare == 0)
        {
            compare = hCost.CompareTo(other.hCost);
        }

        return compare; // 直接返回正确比较结果
    }

}