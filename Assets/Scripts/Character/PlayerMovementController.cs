using Assets.Scripts.Networking;
using Assets.Scripts.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerAnimationController))]
public class PlayerMovementController : MovementControllerBase
{
    [Header("Movement Settings")]
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackCooldown = 0.8f;

    private Transform targetEnemy;
    private bool isMovingToEnemy = false;

    private bool isAttacking = false;
    private float attackTimer = 0f;
    private float attackCooldownTimer = 0f;

    private Rigidbody2D rb;
    private Vector2 moveAxisInput;
    private PlayerAnimationController anim;
    private float attackAnimDuration = 0.4f;
    private Vector2 moveInput;
    private bool isGettingHit = false;

    protected override void Start()
    {
        base.Start();
        // moveSpeed có thể được set trong inspector; nếu cần khác có thể set ở đây.
    }

    private void Awake()
    {
        anim = GetComponent<PlayerAnimationController>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnMove(InputValue input)
    {
        moveInput = input.Get<Vector2>();
    }

    private void FixedUpdate()
    {
        if (!GameManager.Instance.CanPlayerMove)
            return;

        if (isGettingHit)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        moveAxisInput = new Vector2(moveInput.x, moveInput.y);

        UpdateAttackTimers();

        if (isMovingToEnemy && targetEnemy != null)
        {
            if (moveAxisInput != Vector2.zero)
            {
                CancelAutoFollow();
                ManualMove();
                return;
            }

            Monster enemy = targetEnemy.GetComponent<Monster>();
            if (enemy == null || enemy.IsDead)
            {
                CancelAutoFollow();
                return;
            }

            AutoMoveToEnemyPath();
            return;
        }

        var mouse = Mouse.current;
        if (mouse.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPos = mouse.position.ReadValue();
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            Collider2D hit = Physics2D.OverlapPoint(worldPos);
            if (hit != null)
            {
                if (hit.CompareTag("Enemy"))
                {
                    ClearPath();
                    targetEnemy = hit.transform;
                    isMovingToEnemy = true;
                }
                else if(hit.CompareTag("NPC"))
                {
                    NPCDialogueController npc=hit.gameObject.GetComponent<NPCDialogueController>();
                    npc.StartDialogue();
                }
            }
        }

        ManualMove();
    }

    public void HandleGetHit()
    {
        if (isGettingHit) return;

        isGettingHit = true;
        CancelAutoFollow();
        anim.SetGetHitAnimation(true);
        rb.linearVelocity = Vector2.zero;

        StartCoroutine(GetHitDelayRoutine());
    }

    private IEnumerator GetHitDelayRoutine()
    {
        yield return new WaitForSeconds(attackAnimDuration);
        isGettingHit = false;
        anim.SetGetHitAnimation(false);
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
            if (currentPath == null || currentPath.Count == 0 || ReachedTargetTile() || pathfinder != null)
        {
            var startNode = pathfinder.GetTileFromWorld(transform.position);
            var endNode = pathfinder.GetTileFromWorld(targetEnemy.position);

            if (startNode == null || endNode == null)
            {
                Debug.LogWarning("[AutoMove] ❌ StartNode or EndNode is null!");
                return;
            }

            // pass the agent's BoxCollider2D size to pathfinder so obstacles are considered with agent's size
            var agentSize = GetAgentSizeForPathfinding();
            var path = pathfinder.FindPath(startNode, endNode, agentSize);
            if (path == null || path.Count == 0)
            {
                return;
            }

            currentPath = path.Select(n => n.worldPos).ToList();
            pathIndex = 0;

            string pathStr = string.Join(" → ", path.Select(p => $"({p.gridPos.x},{p.gridPos.y})"));
        }

        MoveAlongPath();
    }


    override protected void MoveAlongPath()
    {
        if (currentPath == null || pathIndex >= currentPath.Count)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector3 targetPos = currentPath[pathIndex];
        Vector3 dir = targetPos - transform.position;

        float dist = dir.magnitude;
        if (dist < 0.1f)
        {
            pathIndex++;
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
        if (!EnsurePathfinder()) return false;

        var playerNode = pathfinder.GetTileFromWorld(transform.position);
        var enemyNode = pathfinder.GetTileFromWorld(targetEnemy.position);
        if (playerNode == null || enemyNode == null) return false;
        return playerNode.gridPos == enemyNode.gridPos;
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
        targetEnemy.gameObject.GetComponent<Monster>().TakeDamage(100, gameObject);
        anim.SetAttackAnimation(true);
    }

    private static long timeStartSendMove = 0;
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

            if (SystemUtil.CurrentTimeMillis() - timeStartSendMove > 2000)
            {
                timeStartSendMove = SystemUtil.CurrentTimeMillis();
                RequestManager.PlayerMove((int)this.transform.position.x, (int)this.transform.position.y);
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
        ClearPath();
    }
}