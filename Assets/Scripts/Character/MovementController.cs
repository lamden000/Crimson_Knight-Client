using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(PlayerAnimationController))]
public class MovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackCooldown = 0.8f;
    [SerializeField] private float moveSpeed = 3f;

    private Pathfinder pathfinder;
    private List<Vector3> currentPath = new List<Vector3>();
    private int currentNodeIndex = 0;

    private Transform targetEnemy;
    private bool isMovingToEnemy = false;

    private bool isAttacking = false;
    private float attackTimer = 0f;
    private float attackCooldownTimer = 0f;

    private Rigidbody2D rb;
    private Vector2 moveAxisInput;
    private PlayerAnimationController anim;
    private float attackAnimDuration = 0.4f;

    private void Awake()
    {
        anim = GetComponent<PlayerAnimationController>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        pathfinder=Pathfinder.Instance;
    }

    private void FixedUpdate()
    {
        // Đọc input tay
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveAxisInput = new Vector2(h, v);

        UpdateAttackTimers();

        if (isMovingToEnemy && targetEnemy != null)
        {
            // cancel nếu có input tay
            if (moveAxisInput != Vector2.zero)
            {
                CancelAutoFollow();
                ManualMove();
                return;
            }

            // nếu enemy chết
            Enemy enemy = targetEnemy.GetComponent<Enemy>();
            if (enemy == null || enemy.IsDead)
            {
                CancelAutoFollow();
                return;
            }

            AutoMoveToEnemyPath();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(worldPos);
            if (hit != null && hit.CompareTag("Enemy"))
            {
              
                currentPath.Clear();
                targetEnemy = hit.transform;
                isMovingToEnemy = true;
            }
        }

        // Điều khiển bằng tay
        ManualMove();
    }

    private void UpdateAttackTimers()
    {
        if (isAttacking)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackAnimDuration)
            {
                isAttacking = false;
                anim.SetAnimation(anim.GetCurrentDirection(), State.Idle);
                anim.SetAttackAnimation(false);
            }
        }

        if (attackCooldownTimer > 0)
            attackCooldownTimer -= Time.deltaTime;
    }


    private void AutoMoveToEnemyPath()
    {
        if (targetEnemy == null)
        {
            Debug.LogWarning("[AutoMove] ❌ No target enemy!");
            return;
        }

        Vector3 dir = targetEnemy.position - transform.position;

        // Nếu trong tầm đánh
        if (dir.magnitude <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;
            TryAttack(dir);
            return;
        }

        // Nếu chưa có path hoặc đã đến node hiện tại → tìm lại đường
        if (currentPath == null || currentPath.Count == 0 || ReachedTargetTile())
        {
            var startNode = pathfinder.GetTileFromWorld(transform.position);
            var endNode = pathfinder.GetTileFromWorld(targetEnemy.position);

            if (startNode == null || endNode == null)
            {
                Debug.LogWarning("[AutoMove] ❌ StartNode or EndNode is null!");
                return;
            }

            var path = pathfinder.FindPath(startNode, endNode);
            if (path == null || path.Count == 0)
            {
                Debug.LogWarning("[AutoMove] ❌ No valid path found!");
                return;
            }

            currentPath = path.Select(n => n.worldPos).ToList();
            currentNodeIndex = 0;

            string pathStr = string.Join(" → ", path.Select(p => $"({p.gridPos.x},{p.gridPos.y})"));
        }

        MoveAlongPath();
    }


    private void MoveAlongPath()
    {
        if (currentPath == null || currentNodeIndex >= currentPath.Count)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector3 targetPos = currentPath[currentNodeIndex];
        Vector3 dir = targetPos - transform.position;

        float dist = dir.magnitude;
        if (dist < 0.1f)
        {
            currentNodeIndex++;
            return;
        }

        dir.Normalize();
        Vector2 nextPos = Vector2.MoveTowards(
            rb.position,
            targetPos,
            moveSpeed * Time.fixedDeltaTime
        );
                rb.MovePosition(nextPos);

        // Animation theo hướng di chuyển
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            anim.SetAnimation(Direction.Left, State.Walk);
            transform.rotation = (dir.x > 0)
                ? Quaternion.Euler(0, 180f, 0)
                : Quaternion.identity;
        }
        else
        {
            if (dir.y > 0)
                anim.SetAnimation(Direction.Up, State.Walk);
            else
                anim.SetAnimation(Direction.Down, State.Walk);
            transform.rotation = Quaternion.identity;
        }
    }


    private bool ReachedTargetTile()
    {
        if (targetEnemy == null) return false;

        var playerNode = pathfinder.GetTileFromWorld(transform.position);
        var enemyNode = pathfinder.GetTileFromWorld(targetEnemy.position);

        if (playerNode == null || enemyNode == null) return false;

        bool reached = playerNode.gridPos == enemyNode.gridPos;
        return reached;
    }



    private void TryAttack(Vector3 dir)
    {
        if (isAttacking || attackCooldownTimer > 0) return;

        isAttacking = true;
        attackTimer = 0f;
        attackCooldownTimer = attackCooldown;
        rb.linearVelocity = Vector2.zero;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            anim.SetAnimation(Direction.Left, State.Attack);
            transform.rotation = (dir.x > 0)
                ? Quaternion.Euler(0, 180f, 0)
                : Quaternion.identity;
        }
        else
        {
            if (dir.y > 0)
                anim.SetAnimation(Direction.Up, State.Attack);
            else
                anim.SetAnimation(Direction.Down, State.Attack);
            transform.rotation = Quaternion.identity;
        }
        anim.SetAttackAnimation(true);
    }

    private void ManualMove()
    {
        float h = moveAxisInput.x;
        float v = moveAxisInput.y;
        bool moving = moveAxisInput != Vector2.zero;

        if (moving)
        {
            if (Mathf.Abs(h) > Mathf.Abs(v))
            {
                rb.linearVelocity = new Vector2(h * moveSpeed, 0);
                anim.SetAnimation(Direction.Left, State.Walk);
                transform.rotation = (h > 0)
                    ? Quaternion.Euler(0, 180f, 0)
                    : Quaternion.identity;
            }
            else
            {
                rb.linearVelocity = new Vector2(0, v * moveSpeed);
                if (v > 0)
                    anim.SetAnimation(Direction.Up, State.Walk);
                else
                    anim.SetAnimation(Direction.Down, State.Walk);
                transform.rotation = Quaternion.identity;
            }
        }
        else
        {
            anim.SetAnimation(anim.GetCurrentDirection(), State.Idle);
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void CancelAutoFollow()
    {
        isMovingToEnemy = false;
        targetEnemy = null;
        currentPath.Clear();
        currentNodeIndex = 0;
    }
}
