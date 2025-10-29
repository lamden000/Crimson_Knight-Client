using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
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
        var agentSize = GetAgentSizeForPathfinding();
        var nodePath = pathfinder.FindPath(startNode, endNode, agentSize);
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

    protected bool AtCurrentNode(float threshold = 1f)
    {
        return Vector3.Distance(transform.position, GetCurrentTargetWorldPos()) < threshold;
    }

    protected void AdvanceNodeIfNeeded(float threshold = 1f)
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

    // Reference to running follow coroutine so children can cancel if needed.
    protected Coroutine followCoroutine;

    // Generic coroutine that moves along currentPath until completion. If a dynamic stopTarget
    // and stopRange are provided, the coroutine will exit early when within stopRange of stopTarget.
    // Child classes must implement MoveAlongPath() to perform a single-step movement.
    protected IEnumerator FollowPath(float arrivalDistance = 1f, Transform stopTarget = null, float stopRange = 0f)
    {
        while (currentPath != null && pathIndex < currentPath.Count)
        {
            // if a dynamic target and range are provided, break when within range
            if (stopTarget != null && stopRange > 0f)
            {
                if (Vector3.Distance(transform.position, stopTarget.position) <= stopRange)
                {
                    yield break;
                }
            }

            if (Vector3.Distance(transform.position, GetCurrentTargetWorldPos()) < arrivalDistance)
            {
                pathIndex++;
                yield return null;
                continue;
            }

            MoveAlongPath();
            yield return null;
        }
    }

    // Start (and cancel previous) follow coroutine. Optional dynamic stop target/range supported.
    protected Coroutine StartFollow(float arrivalDistance = 1f, Transform stopTarget = null, float stopRange = 0f)
    {
        if (followCoroutine != null)
        {
            StopCoroutine(followCoroutine);
            followCoroutine = null;
        }
        followCoroutine = StartCoroutine(FollowPath(arrivalDistance, stopTarget, stopRange));
        return followCoroutine;
    }

    // Build path to a static world position and follow it until arrival.
    protected IEnumerator MoveToTarget(Vector3 targetWorldPos, float arrivalDistance = 1f)
    {
        if (!EnsurePathfinder()) yield break;

        var startNode = pathfinder.GetTileFromWorld(transform.position);
        var endNode = pathfinder.GetTileFromWorld(targetWorldPos);
        if (startNode == null || endNode == null) yield break;

        var nodePath = pathfinder.FindPath(startNode, endNode);
        if (nodePath == null || nodePath.Count == 0) yield break;

        // set current path and start following
        currentPath = nodePath.Select(n => n.worldPos).ToList();
        pathIndex = 0;

        // run follow coroutine and keep reference so callers can cancel/check it
        followCoroutine = StartCoroutine(FollowPath(arrivalDistance));
        yield return followCoroutine;
        followCoroutine = null;
    }

    // Build path to a dynamic target (Transform) and follow it until within stopRange of the target.
    protected IEnumerator MoveToTarget(Transform target, float stopRange, float arrivalDistance = 1f)
    {
        if (target == null) yield break;
        if (!EnsurePathfinder()) yield break;

        var startNode = pathfinder.GetTileFromWorld(transform.position);
        var endNode = pathfinder.GetTileFromWorld(target.position);
        if (startNode == null || endNode == null) yield break;

        var agentSize = GetAgentSizeForPathfinding();
        var nodePath = pathfinder.FindPath(startNode, endNode, agentSize);
        if (nodePath == null || nodePath.Count == 0) yield break;

        SetCurrentPathFromNodes(nodePath);
        pathIndex = 0;

        // run follow coroutine and keep reference so callers can cancel/check it
        followCoroutine = StartCoroutine(FollowPath(arrivalDistance, target, stopRange));
        yield return followCoroutine;
        followCoroutine = null;
    }

    protected void ClearPath()
    {
        currentPath?.Clear();
        pathIndex = 0;
    }
}