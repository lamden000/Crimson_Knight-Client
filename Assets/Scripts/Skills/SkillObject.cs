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
    private bool exploded = false;
    private bool isExplosive = false;
    public float skyHeight = 200f;
    public void Init(SkillObjectData d,Vector3 casterPos ,Vector3 mousePos, bool isExplosive, Transform targetFollow = null)
    {
        data = d;
        this.target = targetFollow;
        this.mousePos = mousePos;
        this.casterPos = casterPos;
        this.isExplosive = isExplosive;

        PlayAnimations();
        StartCoroutine(AutoExplodeTimer());
        StartCoroutine(RunMovement());
    }

    void PlayAnimations()
    {
        // MAIN
        mainAnim.Play(data.mainFrames, data.mainFPS,!data.mainLoop);

        // AFTERMATH (ảnh hưởng sau nổ)
        aftermathAnim.sr.enabled = false;
    }

    IEnumerator RunMovement()
    {
        switch (data.movementType)
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

            case SkillMovementType.ProjectileFromSky:
                yield return ProjectileFromSky();
                break;
        }
    }

    IEnumerator Projectile()
    {
        Vector2 dir = mousePos - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle + 90f);
        while (Vector3.Distance(transform.position, mousePos) > 1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                mousePos,
                data.speed * Time.deltaTime
            );
            yield return null;
        }
        yield return Explosion();
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

    IEnumerator ProjectileFromSky()
    {
        // Lấy vị trí target cần rơi xuống
        Vector3 targetPos = mousePos;

        Vector3 startSkyPos = new Vector3(targetPos.x, targetPos.y + skyHeight, targetPos.z);

        // Chuyển object lên trời (startPos trong Init sẽ bị override)
        transform.position = startSkyPos;

        while (Vector3.Distance(transform.position, targetPos) > 1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
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
                afterCo = aftermathAnim.Play(data.aftermathFrames, data.aftermathFPS);
            }

            // SPARKLE
            if (data.sparkleFrames != null && data.sparkleFrames.Length > 0)
            {
                sparkleCo = sparkleAnim.Play(data.sparkleFrames, data.sparkleFPS);
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
        Destroy(gameObject);
    }


    IEnumerator AutoExplodeTimer()
    {
        yield return new WaitForSeconds(data.autoExplosionTime);

        if (!exploded)
            StartCoroutine(Explosion());
    }
}
