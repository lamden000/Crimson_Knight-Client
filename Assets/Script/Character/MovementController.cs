using UnityEngine;

[RequireComponent(typeof(AnimationController))]
public class MovementController : MonoBehaviour
{

    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackCooldown = 0.8f;
    [SerializeField] private float moveSpeed = 3f;

    private Transform targetEnemy;
    private bool isMovingToEnemy = false;

    private bool isAttacking = false;
    private float attackTimer = 0f;
    private float attackCooldownTimer = 0f;

    private Rigidbody2D rb;

    private Vector2 moveAxisInput;
    private AnimationController anim;
    float attackAnimDuration = 0.4f;


    private void Awake()
    {
        anim = GetComponent<AnimationController>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // đọc input
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveAxisInput = new Vector2(h, v);

        // update attack timer
        if (isAttacking)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackAnimDuration)
            {
                isAttacking = false;
                // khi hết animation Attack thì chuyển về Idle nhưng vẫn giữ cooldown
                anim.SetAnimation(anim.GetCurrentDirection(),State.Idle);
                anim.SetAttackAnimation(false);
            }
        }

        if (attackCooldownTimer > 0)
            attackCooldownTimer -= Time.deltaTime;

        // nếu đang follow enemy
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

            AutoMoveToEnemy();
            return;
        }

        // click chuột trái chọn enemy
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(worldPos);

            if (hit != null && hit.CompareTag("Enemy"))
            {
                targetEnemy = hit.transform;
                isMovingToEnemy = true;
            }
        }

        // điều khiển tay
        ManualMove();
    }

    private void AutoMoveToEnemy()
    {
        Vector3 dir = targetEnemy.position - transform.position;

        // nếu trong tầm đánh
        if (dir.magnitude <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;
            TryAttack(dir);
            return;
        }

        // --- di chuyển (ưu tiên X trước, rồi Y) ---
        if (Mathf.Abs(dir.x) > 0.1f)
        {
            rb.linearVelocity = new Vector2(Mathf.Sign(dir.x) * moveSpeed, 0);

            anim.SetAnimation(Direction.Left, State.Walk);
            transform.rotation = (dir.x > 0)
                ? Quaternion.Euler(0, 180f, 0)
                : Quaternion.identity;
        }
        else
        {
            rb.linearVelocity = new Vector2(0, Mathf.Sign(dir.y) * moveSpeed);

            if (dir.y > 0)
                anim.SetAnimation(Direction.Up, State.Walk);
            else
                anim.SetAnimation(Direction.Down, State.Walk);

            transform.rotation = Quaternion.identity;
        }
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
    }

}
