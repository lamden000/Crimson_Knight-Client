using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class MovementControllerBase : MonoBehaviour
{
    [Header("Movement (base)")]
    public float moveSpeed = 3f;

    protected Pathfinder pathfinder;
    protected List<Vector3> currentPath = new List<Vector3>();
    protected int pathIndex = 0;

    protected virtual void Start()
    {
        pathfinder = Pathfinder.Instance;
    }

    // Helper to get agent size (BoxCollider2D) in world units for pathfinding queries.
    // Default returns Vector2.zero which indicates "no size provided" and will fallback to simple checks.
    protected Vector2 GetAgentSizeForPathfinding()
    {
        var box = GetComponent<BoxCollider2D>();
        if (box == null) return Vector2.zero;

        // collider size in local units scaled by transform.lossyScale to get world units
        Vector2 size = new Vector2(box.size.x * transform.lossyScale.x, box.size.y * transform.lossyScale.y);
        return size;
    }

    protected bool EnsurePathfinder()
    {
        if (pathfinder == null)
            pathfinder = Pathfinder.Instance;
        return pathfinder != null;
    }

    // Tạo path từ vị trí hiện tại tới worldPos (trả về danh sách Vector3 world positions)
    protected virtual List<Vector3> BuildPathToWorld(Vector3 targetWorldPos)
    {
        if (!EnsurePathfinder()) return null;
        var startNode = pathfinder.GetTileFromWorld(transform.position);
        var endNode = pathfinder.GetTileFromWorld(targetWorldPos);
        if (startNode == null || endNode == null) return null;
        var nodePath = pathfinder.FindPath(startNode, endNode);
        if (nodePath == null || nodePath.Count == 0) return null;
        return nodePath.Select(n => n.worldPos).ToList();
    }

    // Thiết lập currentPath từ danh sách TileNode (dùng khi pathfinder trả về nodes)
    protected virtual void SetCurrentPathFromNodes(List<TileNode> nodePath)
    {
        if (nodePath == null)
        {
            currentPath = null;
            pathIndex = 0;
            return;
        }
        currentPath = nodePath.Select(n => n.worldPos).ToList();
        pathIndex = 0;
    }

    protected Vector3 GetCurrentTargetWorldPos()
    {
        if (currentPath == null || currentPath.Count == 0)
            return transform.position;
        if (pathIndex >= currentPath.Count)
            return currentPath[currentPath.Count - 1];
        return currentPath[pathIndex];
    }

    protected bool AtCurrentNode(float threshold = 0.1f)
    {
        return Vector3.Distance(transform.position, GetCurrentTargetWorldPos()) < threshold;
    }

    protected void AdvanceNodeIfNeeded(float threshold = 0.1f)
    {
        if (currentPath == null) return;
        if (AtCurrentNode(threshold))
            pathIndex++;
    }

    // Child override để thực hiện di chuyển thực tế (rigidbody vs transform)
    protected virtual void MoveAlongPath()
    {
        // default no-op; child should override
    }

    protected void ClearPath()
    {
        currentPath?.Clear();
        pathIndex = 0;
    }
}