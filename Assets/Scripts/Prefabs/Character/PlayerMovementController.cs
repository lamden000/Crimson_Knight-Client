using Assets.Scripts.Networking;
using Assets.Scripts.Utils;
using NavMeshPlus.Components;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
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

    public Rigidbody2D rb;
    private Vector2 moveAxisInput;
    private PlayerAnimationController anim;
    private float attackAnimDuration = 0.4f;
    private Vector2 moveInput;
    private bool isGettingHit = false;
   


    public bool IsMainPlayer = false;

    protected override void Start()
    {
        base.Start();
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
        if (isGettingHit)
        {
            desiredVelocity = Vector2.zero;
            return;
        }
        moveAxisInput = new Vector2(moveInput.x, moveInput.y);

        UpdateAttackTimers();
       
        var mouse = Mouse.current;
        var touchScreen = Touchscreen.current;


        if (IsMainPlayer)
        {
            moveAxisInput = new Vector2(moveInput.x, moveInput.y);
            ManualMove();
        }
    }


    public void MoveToXY(int x, int y)
    {
        CancelAutoFollow();

        Vector3 targetWorldPos = new Vector3(x, y, 0);

        if (!EnsurePathfinder())
        {
            Debug.LogError("[MoveToXY] Pathfinder không tồn tại!");
            return;
        }

        var startNode = pathfinder.GetTileFromWorld(transform.position);
        var endNode = pathfinder.GetTileFromWorld(targetWorldPos);

        if (startNode == null || endNode == null)
        {
            Debug.LogWarning("[MoveToXY] Không tìm thấy start hoặc end node!");
            return;
        }

        var agentSize = GetAgentSizeFromCollider(boxCollider);

        // Tìm đường
        var nodePath = pathfinder.FindPath(startNode, endNode, agentSize);

        if (nodePath == null || nodePath.Count == 0)
        {
            Debug.LogWarning("[MoveToXY] Không tìm thấy path!");
            return;
        }

        SetCurrentPathFromNodes(nodePath);

        StartFollow(arrivalDistance, 0f);
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
            }
        }

        if (attackCooldownTimer > 0)
            attackCooldownTimer -= Time.deltaTime;
    }


    override protected void MoveAlongPath()
    {
        if (currentPath == null || pathIndex >= currentPath.Count)
        {
            desiredVelocity = Vector2.zero;
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
        // only update desiredVelocity here; actual position change happens in FixedUpdate of base
        desiredVelocity = new Vector2(dir.x, dir.y) * moveSpeed;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            Direction direction = dir.x > 0 ? Direction.Right : Direction.Left;
            anim.SetAnimation(direction, State.Walk);
        }
        else
        {
            if (dir.y > 0)
                anim.SetAnimation(Direction.Up, State.Walk);
            else
                anim.SetAnimation(Direction.Down, State.Walk);
        }
    }

    bool flag = false;
    private void ManualMove()
    {
        float h = moveAxisInput.x;
        float v = moveAxisInput.y;
        bool moving = moveAxisInput != Vector2.zero;

        if (moving)
        {
            flag = true;
            
            if (Mathf.Abs(h) > Mathf.Abs(v))
            {
                desiredVelocity = new Vector2(h * moveSpeed, 0);
                Direction direction = (h > 0) ? Direction.Right : Direction.Left;
                anim.SetAnimation(direction, State.Walk);
            }
            else
            {
                desiredVelocity = new Vector2(0, v * moveSpeed);
                if (v > 0)
                    anim.SetAnimation(Direction.Up, State.Walk);
                else
                    anim.SetAnimation(Direction.Down, State.Walk);
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
            desiredVelocity = Vector2.zero;
            if (flag)
            {
                RequestManager.PlayerMove((int)this.transform.position.x, (int)this.transform.position.y);
                flag = false;
            }
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
       
        ClearPath();
    }

    protected override void OnPathFinished()
    {
        anim.SetAnimation(anim.GetCurrentDirection(), State.Idle);
    }
}