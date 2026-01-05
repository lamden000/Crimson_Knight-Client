using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class SkillObject : MonoBehaviour
{
    public SkillObjectData data;

    public SkillAnimation mainAnim;
    public SkillAnimation sparkleAnim;
    public SkillAnimation aftermathAnim;

    private Transform target;
    private Vector3 mousePos;
    private Vector3 casterPos;
    private Coroutine mainAnimCor;
    private SkillMovementType movementType;
    public bool exploded { get; private set; } = false;

    private bool isExplosive = false;
    public float skyHeight = 200f;
    public System.Action<SkillObject> onExplode;
    public void Init(SkillObjectData d,Vector3 casterPos ,Vector3 mousePos, bool isExplosive,SkillMovementType movementType, Transform targetFollow = null)
    {
        data = d;
        this.target = targetFollow;
        this.mousePos = mousePos;
        this.casterPos = casterPos;
        this.isExplosive = isExplosive;
        this.movementType = movementType;
        transform.localScale= data.scale;

        StartCoroutine(RoutineLogic());
    }

    IEnumerator RoutineLogic()
    {
        // ACTIVE PHASE
        PlayAnimations();
        StartCoroutine(AutoExplodeTimer());
        yield return StartCoroutine(RunMovement());
    }

    void PlayAnimations()
    {
        // MAIN
        mainAnimCor= mainAnim.Play(data.mainFrames, data.mainFPS,data.mainLoop,data.autoDisableAfterMain, 0f, data.mainSortingOrder);

        // AFTERMATH (ảnh hưởng sau nổ)
        aftermathAnim.sr.enabled = false;
    }

    IEnumerator RunMovement()
    {
        switch (movementType)
        {
            case SkillMovementType.Projectile:
                yield return Projectile();
                break;

            case SkillMovementType.Homing:
                if(target == null)
                {
                    yield return Projectile();
                }
                yield return Homing();
                break;

            case SkillMovementType.PersistentArea:
                yield return PersistentAreaRoutine();
                break;

            case SkillMovementType.AttachedToTarget:
                yield return AttachedToTargetRoutine();
                break;
        }
    }

    IEnumerator AttachedToTargetRoutine()
    {
        // Nếu không có target thì nổ luôn
        if (target == null)
        {
            yield return Explosion();
            yield break;
        }

        // Tính offset ban đầu dựa trên vị trí spawn (do SpawnPattern quyết định) so với target
        Vector3 offset = transform.position - target.position;

        while (!exploded && target != null)
        {
            // Cập nhật vị trí theo target + offset ban đầu
            transform.position = target.position + offset;
            yield return null;
        }

        // Nếu target chết/mất mà chưa nổ -> nổ
        if (!exploded)
        {
            yield return Explosion();
        }
    }

    IEnumerator PersistentAreaRoutine()
    {
       // Nếu mainLoop = true → main animation sẽ loop mãi → không bao giờ explode
        // nên tôi fallback: nếu mainLoop = true → explode sau autoExplosionTime
        if (data.mainLoop)
        {
            yield return new WaitForSeconds(data.autoExplosionTime);
            yield return Explosion();
            yield break;
        }

        // Đợi main animation chạy hết
        if (mainAnimCor != null)
            yield return mainAnimCor;

        // Nổ ngay sau main animation
        yield return Explosion();
    }


    IEnumerator Projectile()
    {
        Vector2 dir;
        if (target != null)
        {
            dir = ((Vector2)target.position - (Vector2)transform.position).normalized;
        }
        else
        {
            // Nếu không có target, bắn theo hướng chuột hoặc mặc định phải
            dir = ((Vector2)mousePos - (Vector2)transform.position).normalized;
            if (dir == Vector2.zero) dir = Vector2.right;
        }

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Xoay theo hướng
        transform.rotation = Quaternion.Euler(0, 0, angle + 90f);

        // ---- AUTO FLIP ----
        Vector3 scale = transform.localScale;
        // Reset scale x dương trước khi flip để tránh lỗi flip liên tục nếu gọi lại
        scale.x = Mathf.Abs(scale.x) * (dir.x >= 0 ? -1 : 1);
        transform.localScale = scale;
        // -------------------

        // Bay thẳng theo hướng dir mãi mãi (hoặc đến khi AutoExplodeTimer gọi Explosion)
        while (!exploded)
        {
            transform.position += (Vector3)dir * data.speed * Time.deltaTime;
            yield return null;
        }
        
        // Lưu ý: Projectile này giờ sẽ phụ thuộc vào AutoExplodeTimer để dừng/nổ
        // chứ không nổ khi chạm target (trừ khi thêm Collider trigger)
    }


    IEnumerator Homing()
    {
        while (target != null && Vector3.Distance(transform.position, target.position) > 1f)
        {
            Vector2 dir = target.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // sprite mặc định nhìn xuống → thêm +90
            transform.rotation = Quaternion.Euler(0, 0, angle + 90f);

            transform.position = Vector3.MoveTowards(
                transform.position,
                target.position,
                data.speed * Time.deltaTime
            );

            yield return null;
        }

        yield return Explosion();
    }

    IEnumerator Explosion()
    {
        exploded = true;

        if (isExplosive)
        {
            Coroutine afterCo = null;
            Coroutine sparkleCo = null;

            mainAnim.gameObject.SetActive(false);

            if (!data.explosionRotatesWithMovement)
                transform.rotation = Quaternion.Euler(0, 0, 0);

            // AFTERMATH
            if (data.aftermathFrames != null && data.aftermathFrames.Length > 0)
            {
                afterCo = aftermathAnim.Play(data.aftermathFrames, data.aftermathFPS,data.aftermathLoop,true,data.aftermathPlayTime, data.aftermathSortingOrder);
            }

            // SPARKLE
            if (data.sparkleFrames != null && data.sparkleFrames.Length > 0)
            {
                sparkleCo = sparkleAnim.Play(data.sparkleFrames, data.sparkleFPS, false, true, 0f, data.sparkleSortingOrder);
            }
            else
            {
                sparkleAnim.sr.enabled = false;
            }

            // Chờ aftermath chạy xong
            if (afterCo != null)
                yield return afterCo;

            // Chờ sparkle chạy xong
            if (sparkleCo != null)
                yield return sparkleCo;
        }
        onExplode?.Invoke(this);
        Destroy(gameObject);
    }


    IEnumerator AutoExplodeTimer()
    {
        yield return new WaitForSeconds(data.autoExplosionTime);

        if (!exploded)
            StartCoroutine(Explosion());
    }
}
