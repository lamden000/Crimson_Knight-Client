using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyMovementController : MovementControllerBase
{
    [Header("Patrol Area Settings")]
    public int R = 5;
    public float waitTime = 1f;
    public float arrivalDistance = 0.1f;

    [Header("Combat Settings")]
    public float attackRange = 100f;
    public float attackCooldown = 2f;
    public float animationDelay = 0.2f;

    private Transform playerTarget;
    private bool isAttackOnCooldown = false;
    private Coroutine chaseCoroutine;

    private TileNode centerNode;
    private Enemy enemy;
    private int minX, maxX, minZ, maxZ;

    // dùng currentPath (List<Vector3>) và pathIndex từ base
    public bool isPatrolling = false;
    public bool isDebugging = false;

    private Coroutine patrolCoroutine;

    override protected void Start()
    {
        base.Start();
        enemy = GetComponent<Enemy>();
        if (!EnsurePathfinder())
        {
            Debug.LogError("Pathfinder Instance chưa có. Kẻ địch không thể tuần tra.");
            enabled = false;
            return;
        }
        StartCoroutine(WaitForPathfinderInitialization());
    }

    public void StartChase(Transform target)
    {
        StopPatrol();
        playerTarget = target;
        if (isDebugging) Debug.Log($"[Chase] Bắt đầu truy đuổi mục tiêu: {target.name}");
        if (chaseCoroutine != null) StopCoroutine(chaseCoroutine);
        chaseCoroutine = StartCoroutine(ChaseRoutine());
    }

    public void HandleGetHit(Transform attacker)
    {
        StopPatrol();
        if (chaseCoroutine != null) StopCoroutine(chaseCoroutine);

        if (isDebugging) Debug.Log("[GetHit] Kẻ địch bị đánh. Dừng mọi Coroutine di chuyển.");

        enemy.SetState(EnemyState.GetHit);

        StartCoroutine(GetHitDelay(attacker));
    }

    private IEnumerator GetHitDelay(Transform attacker)
    {
        if (isDebugging) Debug.Log($"[GetHitDelay] Bắt đầu chờ animation GetHit ({animationDelay}s).");
        yield return new WaitForSeconds(animationDelay);

        if (attacker != null)
        {
            if (isDebugging) Debug.Log("[GetHitDelay] Animation GetHit kết thúc. Bắt đầu CHASE.");
            enemy.SetState(EnemyState.Idle);
            StartChase(attacker);
        }
        else
        {
            if (isDebugging) Debug.Log("[GetHitDelay] Không có Attacker. Quay lại Patrol.");
            enemy.SetState(EnemyState.Idle);
            patrolCoroutine = StartCoroutine(PatrolRoutine());
        }
    }

    private IEnumerator ChaseRoutine()
    {
        if (isDebugging) Debug.Log("[ChaseRoutine] Bắt đầu vòng lặp truy đuổi.");

        while (playerTarget != null && enemy.currentState != EnemyState.GetHit)
        {
            float distance = Vector3.Distance(transform.position, playerTarget.position);

            if (distance <= attackRange && !isAttackOnCooldown)
            {
                if (isDebugging) Debug.Log($"[ChaseRoutine] Khoảng cách {distance:F2} <= AttackRange. Bắt đầu TẤN CÔNG.");
                yield return StartCoroutine(AttackAction());
            }
            else if (distance > attackRange)
            {
                if (isDebugging) Debug.Log($"[ChaseRoutine] Khoảng cách {distance:F2} > AttackRange. Bắt đầu DI CHUYỂN.");
                yield return StartCoroutine(MoveToTarget(playerTarget.position));
            }
            else
            {
                if (isDebugging) Debug.Log("[ChaseRoutine] Đang chờ Cooldown. Trạng thái Idle.");
                enemy.SetState(EnemyState.Idle);
                yield return null;
            }
            yield return null;
        }
        if (isDebugging) Debug.Log("[ChaseRoutine] Vòng lặp truy đuổi kết thúc.");
    }

    private IEnumerator AttackAction()
    {
        if (isDebugging) Debug.Log("[AttackAction] Thiết lập Attack. Cooldown BẮT ĐẦU.");

        ClearPath();
        enemy.SetState(EnemyState.Attack);
        isAttackOnCooldown = true;

        FlipSprite(playerTarget.position.x > transform.position.x);

        yield return new WaitForSeconds(animationDelay);

        playerTarget.gameObject.GetComponent<PlayerMovementController>().HandleGetHit();

        enemy.SetState(EnemyState.Idle);

        if (isDebugging) Debug.Log($"[AttackAction] Bắt đầu Cooldown: {attackCooldown}s.");
        yield return new WaitForSeconds(attackCooldown - animationDelay);
        isAttackOnCooldown = false;
        if (isDebugging) Debug.Log("[AttackAction] Cooldown KẾT THÚC.");
    }

    private IEnumerator MoveToTarget(Vector3 targetWorldPos)
    {
        if (!EnsurePathfinder()) yield break;

        TileNode startNode = pathfinder.GetTileFromWorld(transform.position);
        TileNode endNode = pathfinder.GetTileFromWorld(targetWorldPos);

        if (startNode == null || endNode == null) yield break;

        var agentSize = GetAgentSizeForPathfinding();
        var nodePath = pathfinder.FindPath(startNode, endNode, agentSize);
        SetCurrentPathFromNodes(nodePath);

        if (currentPath == null || currentPath.Count <= 1)
        {
            enemy.SetState(EnemyState.Idle);
            yield break;
        }

        enemy.SetState(EnemyState.Walk);

        while (pathIndex < currentPath.Count)
        {
            if (Vector3.Distance(transform.position, GetCurrentTargetWorldPos()) < arrivalDistance)
            {
                pathIndex++;
            }
            MoveAlongPath();
            yield return null;

            if (playerTarget != null && Vector3.Distance(transform.position, playerTarget.position) <= attackRange)
            {
                break;
            }
        }
        enemy.SetState(EnemyState.Idle);
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
        GetComponent<Enemy>().SetState(EnemyState.Idle);
        ClearPath();
        if (isDebugging) Debug.Log("[Patrol] Tuần tra đã bị ngắt.");
    }

    void Update()
    {
        if (isPatrolling && currentPath != null && pathIndex < currentPath.Count)
        {
            MoveAlongPath();
        }
    }

    void CalculatePatrolBounds()
    {
        int centerX = centerNode.gridPos.x;
        int centerZ = centerNode.gridPos.y;
        minX = centerX;
        minZ = centerZ;

        maxX = centerX + R - 1;
        maxZ = centerZ + R - 1;

        if (isDebugging)
            Debug.Log($". Center Node: {centerX},{centerZ}; Khu vực tuần tra {R}x{R} đã thiết lập: X [{minX}, {maxX}], Z [{minZ}, {maxZ}]. Tổng số tile: {R * R}");
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

    IEnumerator PatrolRoutine()
    {
        isPatrolling = true;

        while (isPatrolling)
        {
            TileNode endNode = GetRandomPatrolNode();

            if (endNode == null)
            {
                enemy.SetState(EnemyState.Idle);
                yield return new WaitForSeconds(waitTime);
                continue;
            }

            TileNode startNode = pathfinder.GetTileFromWorld(transform.position);

            if (startNode == null || startNode == endNode)
            {
                enemy.SetState(EnemyState.Idle);
                yield return new WaitForSeconds(waitTime);
                continue;
            }

            var nodePath = pathfinder.FindPath(startNode, endNode);
            SetCurrentPathFromNodes(nodePath);
            pathIndex = 0;

            if (currentPath == null || currentPath.Count <= 1)
            {
                if (isDebugging)
                    Debug.LogWarning($"Không tìm thấy đường đi từ {startNode.gridPos} đến {endNode.gridPos}.");

                enemy.SetState(EnemyState.Idle);
            }
            else
            {
                if (isDebugging)
                    Debug.Log($"Bắt đầu di chuyển ngẫu nhiên đến node: {endNode.gridPos}");

                enemy.SetState(EnemyState.Walk);
                yield return StartCoroutine(FollowPath());

                enemy.SetState(EnemyState.Idle);
            }

            yield return new WaitForSeconds(waitTime);
        }
    }

    IEnumerator FollowPath()
    {
        while (currentPath != null && pathIndex < currentPath.Count)
        {
            if (enemy.currentState != EnemyState.Walk)
                enemy.SetState(EnemyState.Walk);

            if (Vector3.Distance(transform.position, GetCurrentTargetWorldPos()) < arrivalDistance)
            {
                pathIndex++;
            }
            yield return null;
        }

        if (isDebugging)
            Debug.Log("Đã hoàn thành một chặng tuần tra ngẫu nhiên.");
    }

    protected override void MoveAlongPath()
    {
        if (currentPath == null || pathIndex >= currentPath.Count) return;

        Vector3 targetWorldPos = GetCurrentTargetWorldPos();
        Vector3 direction = targetWorldPos - transform.position;
        transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.deltaTime);

        if (Mathf.Abs(direction.x) > 0.01f)
        {
            FlipSprite(direction.x > 0);
        }
    }

    private void FlipSprite(bool flip)
    {
        Vector3 scale = transform.localScale;
        float targetScaleX = flip ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        if (scale.x != targetScaleX)
        {
            scale.x = targetScaleX;
            transform.localScale = scale;
        }
    }
}