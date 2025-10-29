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
    private static long timeStartSendMove = 0;
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
    [Header("Interaction")]
    [SerializeField] private float npcInteractRange = 1.2f;
    private Coroutine npcTalkCoroutine;
    private NPCDialogueController pendingNpcDialogue;

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

    private void Update()
    {
        if (!GameManager.Instance.CanPlayerMove)
        {
            rb.linearVelocity = Vector2.zero;
            CancelAutoFollow();
            if (npcTalkCoroutine != null)
            {
                StopCoroutine(npcTalkCoroutine);
                npcTalkCoroutine = null;
                pendingNpcDialogue = null;
            }
            anim.SetAnimation(anim.GetCurrentDirection(), State.Idle);
            return;
        }

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
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Camera cam=Camera.main;
            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector3 screenToWorld = cam.ScreenToWorldPoint(screenPos);
            Vector2 worldPos = new Vector2(screenToWorld.x, screenToWorld.y);
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    ClearPath();
                    targetEnemy = hit.transform;
                    isMovingToEnemy = true;
                }
                else if (hit.collider.CompareTag("NPC"))
                {
                    var npc = hit.collider.GetComponent<NPCDialogueController>();
                    if (npc == null) return;

                    // cancel any existing movement/dialogue interaction
                    CancelAutoFollow();
                    if (npcTalkCoroutine != null)
                    {
                        StopCoroutine(npcTalkCoroutine);
                        npcTalkCoroutine = null;
                        pendingNpcDialogue = null;
                    }

                    // start moving toward NPC and speak when within range
                    pendingNpcDialogue = npc;
                    npcTalkCoroutine = StartCoroutine(MoveToNPCAndTalk(npc));
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

        // Nếu chưa có path hoặc đã đến node hiện tại → tìm lại đường và bắt đầu coroutine follow
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

            SetCurrentPathFromNodes(path);
            pathIndex = 0;

            // start base follow coroutine, but stop early when within attackRange of the enemy
            StartFollow(1f, targetEnemy, attackRange);
            return;
        }
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

    private void ManualMove()
    {
        float h = moveAxisInput.x;
        float v = moveAxisInput.y;
        bool moving = moveAxisInput != Vector2.zero;

        if (moving)
        {
            // manual input cancels any pending NPC interaction
            if (npcTalkCoroutine != null)
            {
                StopCoroutine(npcTalkCoroutine);
                npcTalkCoroutine = null;
                pendingNpcDialogue = null;
            }
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
        // stop any running follow coroutine from base
        if (followCoroutine != null)
        {
            StopCoroutine(followCoroutine);
            followCoroutine = null;
        }
        // stop any pending NPC interaction
        if (npcTalkCoroutine != null)
        {
            StopCoroutine(npcTalkCoroutine);
            npcTalkCoroutine = null;
            pendingNpcDialogue = null;
        }
        ClearPath();
    }

    private IEnumerator MoveToNPCAndTalk(NPCDialogueController npc)
    {
        if (npc == null) yield break;

        // if already in range, start immediately
        if (Vector3.Distance(transform.position, npc.transform.position) <= npcInteractRange)
        {
            npc.StartDialogue();
            npcTalkCoroutine = null;
            pendingNpcDialogue = null;
            yield break;
        }

        // use base MoveToTarget that stops when within npcInteractRange
        yield return StartCoroutine(MoveToTarget(npc.transform, npcInteractRange));

        // after arriving, ensure npc still exists then start dialogue
        if (npc != null)
        {
            npc.StartDialogue();
        }

        npcTalkCoroutine = null;
        pendingNpcDialogue = null;
    }
}