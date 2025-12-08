using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class Skill : MonoBehaviour
{
    public SkillData spawnData;

    private Vector3 casterPos;
    private Vector3 mousePos;
    private Transform target;
    public GameObject skillObjectPrefab;
    public float skyHeight = 200f;
    private bool stopGeneratingExplosive = false;
    private int currentEntryIndex = 0;

    public void Init(
        SkillData spData,
        Vector3 casterPosition,
        Vector3 mousePosition,
        Transform targetFollow = null)
    {
        spawnData = spData;

        casterPos = casterPosition;
        mousePos = mousePosition;
        target = targetFollow;

        // Skill cha KHÔNG còn tự spawn theo spawnPoint nữa → spawnAt quyết định hết
        transform.position = casterPosition;

        if (spawnData != null)
            SpawnChildren();
    }


    // ===============================================================
    //               SPAWN CHILDREN USING SpawnEntry
    // ===============================================================
    void SpawnChildren()
    {
        if (spawnData.spawnEntries == null)
            return;

        for (int i = 0; i < spawnData.spawnEntries.Count; i++)
        {
            stopGeneratingExplosive = false;
            currentEntryIndex = i;
            SpawnWithPattern(spawnData.spawnEntries[i]);
        }  
        Destroy(gameObject);
    }

    // ===============================================================
    //                   DECIDE SPAWN ORIGIN
    // ===============================================================
    Vector3 GetOrigin(SpawnEntry entry)
    {
        switch (entry.origin)
        {
            case SpawnOrigin.CasterPosition:
                return casterPos;

            case SpawnOrigin.TargetPosition:
                return target ? target.position : mousePos;

            case SpawnOrigin.MousePosition:
                return mousePos;

            default:
            case SpawnOrigin.Sky:
                return new Vector3(mousePos.x, mousePos.y +skyHeight, 0f);
        }
    }

    // ===============================================================
    //                       SPAWN PATTERNS
    // ===============================================================
    void SpawnWithPattern(SpawnEntry entry)
    {
        Vector3 origin = GetOrigin(entry);

        switch (entry.pattern)
        {
            case SpawnPattern.None:
                SpawnSingle(entry.skillToSpawn, origin);
                break;

            case SpawnPattern.Circle:
            case SpawnPattern.Radial:
                SpawnCircle(entry, origin);
                break;

            case SpawnPattern.RandomCircle:
                SpawnRandomCircle(entry, origin);
                break;
        }
    }

    void SpawnSingle(SkillObjectData obj, Vector3 pos)
    {
        CreateSkillInstance(obj, pos);
    }

    void SpawnCircle(SpawnEntry entry, Vector3 origin)
    {
        float step = 360f / entry.count;

        for (int i = 0; i < entry.count; i++)
        {
            float angle = (step * i + entry.angleOffset) * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * entry.radius,
                Mathf.Sin(angle) * entry.radius,
                0f
            );

            CreateSkillInstance(entry.skillToSpawn, origin + offset);
        }
    }

    void SpawnRandomCircle(SpawnEntry entry, Vector3 origin)
    {
        for (int i = 0; i < entry.count; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float r = Mathf.Sqrt(Random.value) * entry.radius;

            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * r,
                Mathf.Sin(angle) * r,
                0f
            );

            CreateSkillInstance(entry.skillToSpawn, origin + offset);
        }
    }


    // ===============================================================
    //               CREATE SKILL INSTANCE (NO AUTOSPAWN)
    // ===============================================================
    void CreateSkillInstance(SkillObjectData data, Vector3 spawnPos)
    {
        var obj = Instantiate(skillObjectPrefab, spawnPos, Quaternion.identity);

        var sk = obj.GetComponent<SkillObject>();
        bool isExplosive = false;
        if (!stopGeneratingExplosive)
        {
            isExplosive = true;
            if (!spawnData.spawnEntries[currentEntryIndex].allObjectExplode)
                stopGeneratingExplosive = true;
        }

        // skill object tự xử lý animation/movement/explode
        sk.Init(data,casterPos,mousePos, isExplosive, target);
    }
}
