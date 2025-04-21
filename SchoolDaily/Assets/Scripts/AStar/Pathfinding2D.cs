using System;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding2D : MonoBehaviour
{
    public static Pathfinding2D Instance;

    [Header("网格设置")]
    public Vector2 gridSize = new Vector2(200, 200);
    public float nodeRadius = 1f;
    public LayerMask obstacleMask;

    private Node[,] grid;
    private float nodeDiameter;
    private int gridSizeX, gridSizeY;

    void Awake()
    {
        Instance = this;
        CreateGrid();
    }

    void CreateGrid()
    {
        // 计算节点数量（自动适配地图尺寸）
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridSize.x / nodeDiameter); // 200/0.5=400节点
        gridSizeY = Mathf.RoundToInt(gridSize.y / nodeDiameter);
        grid = new Node[gridSizeX, gridSizeY];

        // 计算网格起点（确保中心对齐）
        Vector2 worldBottomLeft = (Vector2)transform.position -
            new Vector2(gridSize.x / 2, gridSize.y / 2);
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector2 worldPoint = worldBottomLeft +
                    Vector2.right * (x * nodeDiameter + nodeRadius) +
                    Vector2.up * (y * nodeDiameter + nodeRadius);

                bool walkable = !Physics2D.OverlapCircle(worldPoint, nodeRadius, obstacleMask);
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    public List<Vector2> FindPath(Vector2 startPos, Vector2 targetPos)
    {
        Debug.Log(startPos);
        Debug.Log(targetPos);
        //
        //
        Node startNode = NodeFromWorldPoint(startPos); ;
        Node targetNode = NodeFromWorldPoint(targetPos); ;
        // 初始化节点数据
        foreach (Node node in grid)
        {
            node.gCost = int.MaxValue;
            node.hCost = 0;
            node.parent = null;
        }

        if (startNode == null || targetNode == null || !targetNode.walkable)
            return null;

        Heap<Node> openSet = new Heap<Node>(gridSizeX * gridSizeY);
        HashSet<Node> closedSet = new HashSet<Node>();

        // 初始化起始节点
        startNode.gCost = 0;
        Debug.Log("s" + startNode.gridX + startNode.gridY);
        Debug.Log("t" + targetNode.gridX + targetNode.gridY);
        startNode.hCost = GetDistance(startNode, targetNode);
        Debug.Log("Distance" + startNode.hCost);
        openSet.Add(startNode);

        int loopCount = 0;
        while (openSet.Count > 0)
        {
            loopCount++;
            Node currentNode = openSet.RemoveFirst();//从 openSet 中取出 fCost 最小的节点
            closedSet.Add(currentNode);//将当前节点添加到 closedSet 中，表示已处理过的节点。

            // 找到目标节点
            if (currentNode == targetNode)
            {
                List<Vector2> path = SimplifyPath(RetracePath(startNode, targetNode));

                // // 输出路径到控制台
                // Debug.Log("找到路径：");
                // foreach (Vector2 point in path)
                // {
                //     Debug.Log($"节点坐标：{point}");
                // }

                return path;
            }

            foreach (Node neighbour in GetNeighbours(currentNode))//找当前节点的邻居节点
            {
                if (!neighbour.walkable||closedSet.Contains(neighbour))
                    continue;
                // 计算新路径代价
                int newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);

                // 发现更优路径
                if (newCostToNeighbour < neighbour.gCost)
                {
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);//计算邻居节点的hcost
                    neighbour.parent = currentNode;

                    // 更新堆内位置
                    if (openSet.Contains(neighbour))
                    {
                        openSet.UpdateItem(neighbour); // 调用堆更新方法
                    }
                    else
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }
        return null; // 未找到路径
    }

    private List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;
        int safetyCounter = gridSizeX * gridSizeY; // 动态安全限制

        HashSet<Node> visited = new HashSet<Node>(); // 防闭环检测

        while (currentNode != startNode && safetyCounter > 0)
        {
            if (visited.Contains(currentNode))
            {
                Debug.LogError("检测到路径闭环！起始点: " + startNode.worldPosition);
                return null;
            }

            path.Add(currentNode);
            visited.Add(currentNode);
            currentNode = currentNode.parent;
            safetyCounter--;
        }

        // 确保添加起始节点
        path.Add(startNode);

        if (safetyCounter <= 0)
            Debug.LogError("Path retrace exceeded safety limit!");

        path.Reverse();
        return path;
    }

    private List<Vector2> SimplifyPath(List<Node> path)
    {
        List<Vector2> waypoints = new List<Vector2>();
        if (path.Count == 0) return waypoints;

        Vector2 directionOld = Vector2.zero;
        waypoints.Add(path[0].worldPosition);

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = (path[i].worldPosition - path[i - 1].worldPosition).normalized;
            if (directionNew != directionOld)
            {
                waypoints.Add(path[i - 1].worldPosition);
            }
            directionOld = directionNew;
        }
        waypoints.Add(path[path.Count - 1].worldPosition);
        return waypoints;
    }

    private List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();
        // 上下左右四个方向
        Vector2Int[] directions = {
        new Vector2Int(0, 1),  // 上
        new Vector2Int(1, 0),  // 右
        new Vector2Int(0, -1), // 下
        new Vector2Int(-1, 0) // 左
        };

        foreach (var dir in directions)
        {
            int checkX = node.gridX + dir.x;
            int checkY = node.gridY + dir.y;

            if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
            {
                neighbours.Add(grid[checkX, checkY]);
            }
        }
        return neighbours;
    }

    private Node NodeFromWorldPoint(Vector2 worldPosition)
    {
        // 获取 pathfinding 物体的全局坐标
        Vector2 pathfindingPosition = transform.position;

        // 计算传入的世界坐标与 pathfinding 物体的相对位置
        Vector2 relativePosition = worldPosition - pathfindingPosition;

        // 根据相对位置计算百分比
        float percentX = (relativePosition.x + gridSize.x / 2) / gridSize.x;
        float percentY = (relativePosition.y + gridSize.y / 2) / gridSize.y;

        // 限制百分比在 0 到 1 之间
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        // 根据百分比计算网格坐标
        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        // 返回对应的网格节点
        return grid[x, y];
    }

    private int GetDistance(Node a, Node b)
    {
        int dstX = Mathf.Abs(a.gridX - b.gridX);
        int dstY = Mathf.Abs(a.gridY - b.gridY);
        return dstX+dstY;
        // if (dstX > dstY)
        //     return 14 * dstY + 10 * (dstX - dstY);
        // return 14 * dstX + 10 * (dstY - dstX);
    }

}