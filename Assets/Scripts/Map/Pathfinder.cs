﻿using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    public static Pathfinder Instance { get; private set; }

    public TileNode[,] grid{ get; private set; }
    private int width;
    private int height;
    // approximate tile world size (in world units)
    private float tileSizeX = 1f;
    private float tileSizeY = 1f;
    private Vector3 originWorldPos = Vector3.zero;

    public float agentSizeMargin=0.7f;

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
        // compute approximate tile size using neighboring nodes (safe-guards in case map is 1x1)
        if (height > 0 && width > 0 && grid[0,0] != null)
        {
            originWorldPos = grid[0,0].worldPos;
            if (width > 1 && grid[0,1] != null)
                tileSizeX = Mathf.Abs(grid[0,1].worldPos.x - grid[0,0].worldPos.x);
            if (height > 1 && grid[1,0] != null)
                tileSizeY = Mathf.Abs(grid[1,0].worldPos.y - grid[0,0].worldPos.y);

            // fallback to 1 unit if detected zero sizes
            if (tileSizeX <= 0f) tileSizeX = 1f;
            if (tileSizeY <= 0f) tileSizeY = 1f;
        }

        Debug.Log(height * width);
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
            if (debugLogs)
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

    public List<TileNode> FindPath(TileNode startNode, TileNode endNode, Vector2 agentSize = default)
    {
        if (startNode == null || endNode == null)
        {
            if (debugLogs)
                Debug.LogWarning("[Pathfinder] Start or End node is null!");
            return null;
        }

        // Check end node traversability taking the agent size into account
        if (!IsNodeTraversable(endNode, agentSize))
        {
            if (debugLogs)
                Debug.Log("[Pathfinder] End node is not traversable for this agent size!");
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
                // ensure neighbor is traversable for this agent size (considers nearby non-walkable tiles)
                if (!IsNodeTraversable(neighbor, agentSize)) continue;

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
        if (debugLogs)
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
            if (debugLogs)
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

    // Returns true if the agent with given box size (world units) can be centered on 'node'
    // without overlapping any non-walkable tile.
    private bool IsNodeTraversable(TileNode node, Vector2 agentSize)
    {
        if (node == null) return false;

        // If no size provided, fall back to simple walkable check
        if (agentSize == Vector2.zero)
            return node.walkable;

        // agent AABB when centered at node.worldPos
        float margin = Mathf.Min(tileSizeX, tileSizeY) * agentSizeMargin;
        Vector3 agentHalf = new Vector3(agentSize.x / 2f , agentSize.y / 2f - margin, 0f);
        Vector3 agentMin = node.worldPos - agentHalf;
        Vector3 agentMax = node.worldPos + agentHalf;

        // Determine grid search bounds in indices, using gridPos as column (x) and row (y)
        int col = node.gridPos.x;
        int row = node.gridPos.y;

        int colRadius = Mathf.CeilToInt((agentSize.x / 2f) / tileSizeX) + 1;
        int rowRadius = Mathf.CeilToInt((agentSize.y / 2f) / tileSizeY) + 1;

        int colMin = col - colRadius;
        int colMax = col + colRadius;
        int rowMin = row - rowRadius;
        int rowMax = row + rowRadius;

        for (int r = rowMin; r <= rowMax; r++)
        {
            for (int c = colMin; c <= colMax; c++)
            {
                if (c < 0 || c >= width || r < 0 || r >= height) continue;
                var tile = grid[r, c];
                if (tile == null) continue;

                if (tile.walkable) continue;

                // compute tile AABB (assume tile.worldPos is center)
                Vector3 tileHalf = new Vector3(tileSizeX / 2f, tileSizeY / 2f, 0f);
                Vector3 tileMin = tile.worldPos - tileHalf;
                Vector3 tileMax = tile.worldPos + tileHalf;

                // AABB overlap test
                bool overlap = !(agentMax.x < tileMin.x || agentMin.x > tileMax.x ||
                                 agentMax.y < tileMin.y || agentMin.y > tileMax.y);

                if (overlap)
                {
                    return false;
                }
            }
        }

        return true;
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
