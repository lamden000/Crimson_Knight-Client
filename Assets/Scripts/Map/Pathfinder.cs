using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    public static Pathfinder Instance { get; private set; }

    public TileNode[,] grid{ get; private set; }
    private int width;
    private int height;

    [Header("Debug Options")]
    [SerializeField] private bool debugLogs = false;
    [SerializeField] private bool drawGizmos = false;
    public static bool IsInitialized { get; private set; } = false;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Init(TileNode[,] gridNodes)
    {
        grid = gridNodes;
        height = grid.GetLength(0);
        width = grid.GetLength(1);
        IsInitialized = true;
    }

    public TileNode GetTile(int x, int z)
    {
        if (z < 0 || z >= width || x < 0 || x >= height)
        {
            if(debugLogs)
             Debug.LogWarning($"[Pathfinder] Tọa độ lưới ({x}, {z}) nằm ngoài ranh giới bản đồ (Width: {width}, Height: {height})");

            return null; 
        }

        return grid[x, z];
    }

    public TileNode GetTileFromWorld(Vector3 worldPos)
    {
        if (grid == null)
        {
            Debug.LogWarning("[Pathfinder] Grid not initialized!");
            return null;
        }

        TileNode closest = null;
        float minDist = float.MaxValue;

        // brute-force tìm node gần nhất (đơn giản, an toàn)
        foreach (var node in grid)
        {
            float dist = Vector3.Distance(worldPos, node.worldPos);
            if (dist < minDist)
            {
                minDist = dist;
                closest = node;
            }
        }

        if (debugLogs)
            Debug.Log($"[Pathfinder] Closest node to {worldPos:F2} is {closest.gridPos} | walkable={closest.walkable}");

        return closest;
    }

    public List<TileNode> FindPath(TileNode startNode, TileNode endNode)
    {
        if (startNode == null || endNode == null)
        {
            Debug.LogWarning("[Pathfinder] Start or End node is null!");
            return null;
        }

        if (!endNode.walkable)
        {
            Debug.Log("[Pathfinder] End node is not walkable!");
            return null;
        }

        var openSet = new List<TileNode> { startNode };
        var cameFrom = new Dictionary<TileNode, TileNode>();
        var gCost = new Dictionary<TileNode, float>();
        var fCost = new Dictionary<TileNode, float>();

        foreach (var node in grid)
        {
            gCost[node] = float.MaxValue;
            fCost[node] = float.MaxValue;
        }

        gCost[startNode] = 0;
        fCost[startNode] = Heuristic(startNode, endNode);

        while (openSet.Count > 0)
        {
            // chọn node có fCost nhỏ nhất
            TileNode current = openSet[0];
            foreach (var n in openSet)
                if (fCost[n] < fCost[current])
                    current = n;

            // nếu đã đến đích
            if (current == endNode)
                return ReconstructPath(cameFrom, current);

            openSet.Remove(current);

            foreach (var neighbor in GetNeighbors(current))
            {
                if (!neighbor.walkable) continue;

                float tentativeG = gCost[current] + Vector3.Distance(current.worldPos, neighbor.worldPos);

                if (tentativeG < gCost[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gCost[neighbor] = tentativeG;
                    fCost[neighbor] = tentativeG + Heuristic(neighbor, endNode);

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        Debug.LogWarning("[Pathfinder] ❌ No path found!");
        return null;
    }

    private List<TileNode> ReconstructPath(Dictionary<TileNode, TileNode> cameFrom, TileNode current)
    {
        List<TileNode> path = new List<TileNode>();
        while (cameFrom.ContainsKey(current))
        {
            path.Add(current);
            current = cameFrom[current];
        }
        path.Reverse();

        if (debugLogs)
        {
            string pathStr = string.Join(" -> ", path.ConvertAll(p => p.gridPos.ToString()));
            Debug.Log($"[Pathfinder] ✅ Path reconstructed ({path.Count} nodes): {pathStr}");
        }

        return path;
    }

    private float Heuristic(TileNode a, TileNode b)
    {
        // Manhattan heuristic
        return Mathf.Abs(a.gridPos.x - b.gridPos.x) + Mathf.Abs(a.gridPos.y - b.gridPos.y);
    }

    private List<TileNode> GetNeighbors(TileNode node)
    {
        List<TileNode> neighbors = new List<TileNode>();
        int x = node.gridPos.x;
        int y = node.gridPos.y;

        int[,] dirs = new int[,]
        {
            { 0, 1 },  // up
            { 0, -1 }, // down
            { -1, 0 }, // left
            { 1, 0 }   // right
        };

        for (int i = 0; i < dirs.GetLength(0); i++)
        {
            int nx = x + dirs[i, 0];
            int ny = y + dirs[i, 1];
            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                neighbors.Add(grid[ny, nx]);
        }

        return neighbors;
    }

    // --- DEBUG GIZMOS ---
    private void OnDrawGizmos()
    {
        if (!drawGizmos || grid == null) return;

        foreach (var node in grid)
        {
            Gizmos.color = node.walkable ? Color.green : Color.red;
            Gizmos.DrawCube(node.worldPos + Vector3.up * 0.1f, Vector3.one * 0.2f);
        }
    }
}
