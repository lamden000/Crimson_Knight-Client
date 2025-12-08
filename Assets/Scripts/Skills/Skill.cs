using UnityEngine;
using System.Collections;

public class Skill : MonoBehaviour
{
    public SkillData data;

    public SkillAnimation mainAnim;
    public SkillAnimation sparkleAnim;
    public SkillAnimation aftermathAnim;

    private Transform target;
    private Vector3 casterPos;
    private Vector3 mousePos;
    private bool exploded = false;
    public float skyHeight = 200f;
    bool isOriginal = false;
    public void Init(SkillData d, Vector3 casterPos, Vector3 mousePos, Transform targetFollow = null, bool isOriginal = true)
    {
        data = d;
        this.casterPos = casterPos;
        this.mousePos = mousePos;
        this.target = targetFollow;
        this.isOriginal = isOriginal;

        if (data.spawnMultiple)
        {
            if (isOriginal)
            {
                HandleMultiSpawn();
            }
            else
            {
                transform.position = casterPos;
            }
        }    
        else if(!data.spawnMultiple)
            transform.position = GetStartPosition();

        PlayAnimations();
        StartCoroutine(AutoExplodeTimer());
        StartCoroutine(RunMovement());
    }

    void HandleMultiSpawn()
    {
        if (data.spawnPattern == SpawnPattern.None) return;
        if (data.spawnCount <= 1) return;

        switch (data.spawnPattern)
        {
            case SpawnPattern.RandomCircle:
                SpawnRandomCircle();
                break;

            case SpawnPattern.Circle:
                SpawnCircle();
                break;
        }
    }

    void SpawnCircle()
    {
        float angleStep = 360f / data.spawnCount;

        for (int i = 0; i < data.spawnCount; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * data.spawnRadius,
                Mathf.Sin(angle) * data.spawnRadius,
                0
            );

            Vector3 spawnPos =GetStartPosition() + offset;

            if (i == 0)
            {
                transform.position = spawnPos;
            }
            else
            {
                var clone = Instantiate(gameObject);
                clone.GetComponent<Skill>().Init(
                    data,
                    spawnPos,
                    mousePos,
                    target,
                    false // clone không được sinh thêm clone nữa
                );
            }
        }
    }


    void SpawnRandomCircle()
    {
        for (int i = 0; i < data.spawnCount+1; i++)
        {
            // random angle 0–360
            float angle = Random.Range(0f, Mathf.PI * 2f);

            // random radius 0–spawnRadius, nhưng phải cân bằng mật độ → dùng sqrt
            float r = Mathf.Sqrt(Random.Range(0f, 1f)) * data.spawnRadius;

            Vector3 offset = new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r, 0);
            Vector3 spawnPos = GetStartPosition()+ offset;

            if (i == 0)
            {
                transform.position = spawnPos;
            }
            else
            {
                var clone = Instantiate(gameObject);
                clone.GetComponent<Skill>().Init(
                    data,
                    spawnPos,
                    mousePos,
                    target,
                    false // clone không được sinh thêm clone nữa
                );
            }
        }
    }


    Vector3 GetStartPosition()
    {
        switch (data.spawnPoint)
        {
            case SkillSpawnPoint.CasterPosition:
                return casterPos;
            case SkillSpawnPoint.TargetPosition:
                if (target != null)
                    return target.position;
                else
                    return mousePos;
            case SkillSpawnPoint.MousePosition:
            return mousePos;
            case SkillSpawnPoint.Sky:
                if (target != null)
                    return new Vector3(target.position.x, target.position.y + skyHeight, 0);
                else
                    return new Vector3(mousePos.x, mousePos.y + skyHeight, 0);
            default: return casterPos;
        }    
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

        Coroutine afterCo = null;
        Coroutine sparkleCo = null;

        if (isOriginal || data.cloneExplodes)
        {
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
