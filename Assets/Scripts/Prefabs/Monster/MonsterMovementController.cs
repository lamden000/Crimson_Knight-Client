using UnityEngine;
using System.Collections;

public class MonsterMovementController : MovementControllerBase
{
    [Header("Patrol Area Settings")]
    public int R = 2;
    public float waitTime = 1f;

    private TileNode centerNode;
    private MonsterPrefab enemy;
    private int minX, maxX, minZ, maxZ;

    public bool isPatrolling = false;
    public bool isDebugging = false;
    public float animationDelay = 0.2f;
    private Coroutine patrolCoroutine;

    override protected void Start()
    {
        base.Start();
        enemy = GetComponent<MonsterPrefab>();
        if (!EnsurePathfinder())
        {
            Debug.LogError("Pathfinder Instance chưa có. Kẻ địch không thể tuần tra.");
            enabled = false;
            return;
        }
        if (IsBoss)
        {
            return;
        }
        StartCoroutine(WaitForPathfinderInitialization());
    }


    public void HandleGetHit()
    {
        StopPatrol();

        if (isDebugging) Debug.Log("[GetHit] Kẻ địch bị đánh. Dừng di chuyển.");

        enemy.SetState(MonsterState.GetHit);

        StartCoroutine(GetHitDelay());
    }

    private IEnumerator GetHitDelay()
    {
        yield return new WaitForSeconds(0.2f);

        StopMoving();

        if (isDebugging) Debug.Log("[GetHitDelay] Animation GetHit kết thúc. Quay lại Patrol.");
        enemy.SetState(MonsterState.Idle);

        if (!isPatrolling && patrolCoroutine == null)
        {
            patrolCoroutine = StartCoroutine(PatrolRoutine());
        }
    }



    IEnumerator WaitForPathfinderInitialization()
    {
        while (!Pathfinder.IsInitialized)
        {
            yield return null;
        }
        
        centerNode = pathfinder.GetTileFromWorld(transform.position);
        if (centerNode == null)
        {
            Debug.LogError("Không thể xác định TileNode tâm sau khi Pathfinder khởi tạo.");
            enabled = false;
            yield break;
        }

        CalculatePatrolBounds();
        patrolCoroutine = StartCoroutine(PatrolRoutine());
    }

    public void StopPatrol()
    {
        if (!isPatrolling) return;

        isPatrolling = false;
        if (patrolCoroutine != null)
        {
            StopCoroutine(patrolCoroutine);
            patrolCoroutine = null;
        }
        GetComponent<MonsterPrefab>().SetState(MonsterState.Idle);
        ClearPath();
        if (isDebugging) Debug.Log("[Patrol] Tuần tra đã bị ngắt.");
    }

    void Update()
    {
        if (IsBoss)
        {
            return;
        }
        // Only perform frame-based MoveAlongPath when not already running the follow coroutine.
        if (isPatrolling && followCoroutine == null && currentPath != null && pathIndex < currentPath.Count)
        {
            MoveAlongPath();
        }
    }

    void CalculatePatrolBounds()
    {
        int centerX = centerNode.gridPos.x;
        int centerZ = centerNode.gridPos.y;

        int halfR = R / 2;

        minX = centerX - halfR;
        minZ = centerZ - halfR;
        maxX = centerX + halfR;
        maxZ = centerZ + halfR;

        if (isDebugging)
            Debug.Log($"Center: ({centerX},{centerZ}); Patrol area {R}x{R} tiles: X [{minX}, {maxX}], Z [{minZ}, {maxZ}]");
    }

    TileNode GetRandomPatrolNode()
    {
        TileNode targetNode = null;
        int maxAttempts = 50;
        int attempt = 0;

        while (targetNode == null && attempt < maxAttempts)
        {
            int randomX = Random.Range(minX, maxX + 1);
            int randomZ = Random.Range(minZ, maxZ + 1);

            TileNode checkNode = pathfinder.GetTile(randomZ, randomX);

            if (checkNode != null && checkNode.walkable)
            {
                if (checkNode != pathfinder.GetTileFromWorld(transform.position))
                {
                    targetNode = checkNode;
                }
            }
            attempt++;
        }

        if (attempt >= maxAttempts)
        {
            if (isDebugging)
                Debug.LogWarning("Không tìm được node tuần tra ngẫu nhiên hợp lệ sau nhiều lần thử.");
        }

        return targetNode;
    }

    private TileNode debugEndNote;

    public bool IsBoss { get; internal set; }

    IEnumerator PatrolRoutine()
    {
        isPatrolling = true;
        int failCount = 0; // số lần liên tiếp không tìm được đường

        while (isPatrolling)
        {
            TileNode endNode = GetRandomPatrolNode();
                debugEndNote = endNode;

            if (endNode == null)
            {
                StopMoving();
                yield return new WaitForSeconds(waitTime);
                continue;
            }

            TileNode startNode = pathfinder.GetTileFromWorld(transform.position);

            if (startNode == null || startNode == endNode)
            {
                StopMoving();
                yield return new WaitForSeconds(waitTime);
                continue;
            }

            var nodePath = pathfinder.FindPath(startNode, endNode, GetAgentSizeFromCollider(boxCollider));
            SetCurrentPathFromNodes(nodePath);
            pathIndex = 0;

            if (currentPath == null || currentPath.Count <= 1)
            {
                failCount++;
                if (isDebugging)
                {
                    Debug.LogWarning($"[PatrolRoutine] Không tìm thấy đường ({failCount} lần liên tiếp) tới node {endNode.gridPos}.Start node walkable: {startNode.walkable}");
                }

                if (failCount >= 3)
                {
                    if (isDebugging)
                        Debug.LogWarning("[PatrolRoutine] Thất bại nhiều lần. Random lại node mới ngay lập tức.");

                    failCount = 0; 
                    yield return null; 
                    continue;
                }

                desiredVelocity = Vector3.zero;
                StopMoving();
                yield return new WaitForSeconds(waitTime);
                continue;
            }

        
            failCount = 0;
            if (isDebugging)
                Debug.Log($"Bắt đầu di chuyển ngẫu nhiên đến node: {endNode.gridPos}");

            enemy.SetState(MonsterState.Walk);

            followCoroutine = StartCoroutine(FollowPath(arrivalDistance,0));
            yield return followCoroutine;
            followCoroutine = null;

            StopMoving();
            yield return new WaitForSeconds(waitTime);
        }
    }

    protected override void MoveAlongPath()
    {
        if (currentPath == null || pathIndex >= currentPath.Count)
        {
            desiredVelocity = Vector2.zero;
            return;
        }

        Vector3 targetWorldPos = GetCurrentTargetWorldPos();
        Vector3 direction = targetWorldPos - transform.position;

        if (direction.sqrMagnitude > 0.01f)
            desiredVelocity = new Vector2(direction.x, direction.y).normalized * moveSpeed;
        else
            desiredVelocity = Vector2.zero;

        if (Mathf.Abs(direction.x) > 0.01f)
        {
            FlipSprite(direction.x > 0);
        }
    }

    public void StopMoving()
    {
        desiredVelocity= Vector2.zero;
        enemy.SetState(MonsterState.Idle);
    }

    public void FlipSprite(bool flip)
    {
        Vector3 scale = transform.localScale;
        float targetScaleX = flip ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        if (scale.x != targetScaleX)
        {
            scale.x = targetScaleX;
            transform.localScale = scale;
        }
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying || !isDebugging)
            return;
        // Chỉ vẽ khi đang có đường đi hợp lệ
        if (currentPath != null && currentPath.Count > 0)
        {
            // Vẽ toàn bộ đường đi (màu vàng)
            Gizmos.color = Color.yellow;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Vector3 from = currentPath[i];
                Vector3 to = currentPath[i+1];
                Gizmos.DrawLine(from, to);
            }

        }
        if(debugEndNote!=null)
        {
            Vector3 targetPos = debugEndNote.worldPos;
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(targetPos, 5f);
        }    
    }

    protected void OnEnable()
    {
        ClearPath();
        desiredVelocity = Vector2.zero;
        StopAllCoroutines();
        if (centerNode != null && pathfinder != null)
        {
            isPatrolling = true;
            patrolCoroutine = StartCoroutine(PatrolRoutine());
        }
    }
}