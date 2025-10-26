using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EnemyMovementController : MonoBehaviour
{
    [Header("Patrol Area Settings")]
    [Tooltip("Bán kính R tính bằng số lượng ô (tile) mà kẻ địch có thể di chuyển ra xa từ tâm.")]
    public int R = 5;

    [Tooltip("Thời gian dừng lại giữa các lần di chuyển (tính bằng giây).")]
    public float waitTime = 1f;
    [Tooltip("Tốc độ di chuyển của kẻ địch")]
    public float moveSpeed = 3f;
    [Tooltip("Khoảng cách tối thiểu để coi là đã đến đích")]
    public float arrivalDistance = 5f;

    [Header("Combat Settings")]
    [Tooltip("Khoảng cách tối đa để có thể đánh người chơi")]
    public float attackRange = 100f;
    [Tooltip("Thời gian giữa các đòn đánh")]
    public float attackCooldown = 2f;
    [Tooltip("Thời gian chờ animation GetHit và Attack kết thúc")]
    public float animationDelay = 0.2f;

    private Transform playerTarget;
    private bool isAttackOnCooldown = false;
    private Coroutine chaseCoroutine;

    private Pathfinder pathfinder;
    private TileNode centerNode;
    private Enemy enemy;
    private int minX, maxX, minZ, maxZ; 

    private List<TileNode> currentPath;
    private int pathIndex = 0;
    public bool isPatrolling = false;

    public bool isDebugging=false;

    private Coroutine patrolCoroutine;

    void Start()
    {
        pathfinder = Pathfinder.Instance;
        enemy = GetComponent<Enemy>();
        if (pathfinder == null)
        {
            Debug.LogError("Pathfinder Instance chưa có. Kẻ địch không thể tuần tra.");
            enabled = false;
            return;   }
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

        // ⭐ FIX VÀ LOG CỦA BƯỚC CHUYỂN QUAN TRỌNG NHẤT ⭐
        if (attacker != null)
        {
            if (isDebugging) Debug.Log("[GetHitDelay] Animation GetHit kết thúc. Bắt đầu CHASE.");

            // Cần đặt lại trạng thái để ChaseRoutine không bị thoát ngay lập tức.
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

        currentPath = null;
        pathIndex = 0;
        enemy.SetState(EnemyState.Attack);
        isAttackOnCooldown = true;

        FlipSprite(playerTarget.position.x > transform.position.x);

        yield return new WaitForSeconds(animationDelay);
        if (isDebugging) Debug.Log("[AttackAction] Animation Attack kết thúc. Gây sát thương.");

        // ... (Logic gây sát thương) ...
        playerTarget.gameObject.GetComponent<MovementController>().HandleGetHit();

        enemy.SetState(EnemyState.Idle);

        if (isDebugging) Debug.Log($"[AttackAction] Bắt đầu Cooldown: {attackCooldown}s.");
        yield return new WaitForSeconds(attackCooldown - animationDelay);
        isAttackOnCooldown = false;
        if (isDebugging) Debug.Log("[AttackAction] Cooldown KẾT THÚC.");
    }

    private IEnumerator MoveToTarget(Vector3 targetWorldPos)
    {
        TileNode startNode = pathfinder.GetTileFromWorld(transform.position);
        TileNode endNode = pathfinder.GetTileFromWorld(targetWorldPos);

        if (startNode == null || endNode == null) yield break;

        currentPath = pathfinder.FindPath(startNode, endNode);
        pathIndex = 0;

        if (currentPath == null || currentPath.Count <= 1)
        {
            enemy.SetState(EnemyState.Idle);
            yield break;
        }

        enemy.SetState(EnemyState.Walk);

        // Giữ Coroutine này chạy cho đến khi đến đích hoặc bị ngắt
        while (pathIndex < currentPath.Count)
        {
            if (Vector3.Distance(transform.position, GetTargetWorldPosition()) < arrivalDistance)
            {
                pathIndex++;
            }
            MoveAlongPath();
            yield return null;

            // Cần kiểm tra lại khoảng cách tấn công trong khi di chuyển
            if (Vector3.Distance(transform.position, playerTarget.position) <= attackRange)
            {
                break; // Thoát vòng lặp để chuyển sang tấn công
            }
        }
        enemy.SetState(EnemyState.Idle); // Dừng lại khi kết thúc MoveToTarget
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

        // ⭐ Dừng Coroutine tuần tra ⭐
        if (patrolCoroutine != null)
        {
            StopCoroutine(patrolCoroutine);
            patrolCoroutine = null;
        }
        GetComponent<Enemy>().SetState(EnemyState.Idle);
        currentPath = null;
        pathIndex = 0;

        if (isDebugging)
            Debug.Log("[Patrol] Tuần tra đã bị ngắt.");
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

        if(isDebugging) 
            Debug.Log($". Center Node: {centerX},{ centerZ}; Khu vực tuần tra {R}x{R} đã thiết lập: X [{minX}, {maxX}], Z [{minZ}, {maxZ}]. Tổng số tile: {R * R}");
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

            currentPath = pathfinder.FindPath(startNode, endNode);
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
        while (currentPath!=null&&pathIndex < currentPath.Count)
        {
            if (currentPath == null) yield break;

            if (enemy.currentState != EnemyState.Walk)
                enemy.SetState(EnemyState.Walk);

            if (Vector3.Distance(transform.position, GetTargetWorldPosition()) < arrivalDistance)
            {
                pathIndex++;
            }
            yield return null;
        }

        if (isDebugging)
            Debug.Log("Đã hoàn thành một chặng tuần tra ngẫu nhiên.");
    }

    void MoveAlongPath()
    {
        Vector3 targetWorldPos = GetTargetWorldPosition();

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

    private Vector3 GetTargetWorldPosition()
    {
        if (pathIndex >= currentPath.Count)
        {
            return currentPath[currentPath.Count - 1].worldPos;
        }
        return currentPath[pathIndex].worldPos;
    }


}
