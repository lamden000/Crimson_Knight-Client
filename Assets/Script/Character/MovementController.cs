using UnityEngine;

[RequireComponent(typeof(AnimationController))]
public class MovementController : MonoBehaviour
{
    private AnimationController anim;
    private Rigidbody2D rb;

    [SerializeField] private float moveSpeed = 3f;
    private bool isAttacking = false;
    private float attackTimer = 0f;
    [SerializeField] private float attackAnimDuration = 0.5f;

    private void Awake()
    {
        anim = GetComponent<AnimationController>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector2 move = new Vector2(h, v).normalized;

        // di chuyển nhân vật
        rb.linearVelocity = move * moveSpeed;

        bool moving = move != Vector2.zero;


        if (isAttacking)
        {
            attackTimer += Time.deltaTime;
            rb.linearVelocity = Vector2.zero;

            if (attackTimer >= attackAnimDuration)
            {
                isAttacking = false;
            }

            return; // không cho code Idle/Walk chạy
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (TryAttackEnemy()) 
            {
                isAttacking = true;
                attackTimer = 0f;
                rb.linearVelocity = Vector2.zero;
                anim.SetAttackAnimation(true);
                return;
            }
        }
        else if (moving)
        {
            if (Mathf.Abs(h) > Mathf.Abs(v))
            {
                if (h > 0)
                {
                    // đi sang phải: play walk left rồi flip
                    anim.SetAnimation(Direction.Left, State.Walk);
                    transform.rotation = Quaternion.Euler(0, 180f, 0);

                }
                else
                {
                    // đi sang trái: play walk left bình thường
                    anim.SetAnimation(Direction.Left, State.Walk);
                    transform.rotation = Quaternion.identity;

                }
            }
            else
            {
                if (v > 0)
                {
                    anim.SetAnimation(Direction.Up, State.Walk);
                    transform.rotation = Quaternion.identity;
                }
                else
                {
                    anim.SetAnimation(Direction.Down, State.Walk);
                    transform.rotation = Quaternion.identity;
                }
            }
        }
        else
        {
            anim.SetAnimation(anim.GetCurrentDirection(), State.Idle);
            rb.linearVelocity = Vector2.zero;
        }
    }
    private bool TryAttackEnemy()
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(worldPos);

        if (hit != null && hit.CompareTag("Enemy"))
        {
            Vector3 dir = hit.transform.position - transform.position;

            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            {
                anim.SetAnimation(Direction.Left, State.Attack);

                if (dir.x > 0)
                    transform.rotation = Quaternion.Euler(0, 180f, 0);
                else
                    transform.rotation = Quaternion.identity;

            }
            else
            {
                if (dir.y > 0)
                {
                    anim.SetAnimation(Direction.Up, State.Attack);
                }
                else
                {
                    anim.SetAnimation(Direction.Down, State.Attack);
                }

                transform.rotation = Quaternion.identity;
            }

            return true;
        }

        return false;
    }
}
