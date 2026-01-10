using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillSpawnmer : MonoBehaviour
{
    public SkillSpawnData spawnData;

    private Vector3 casterPos;
    private Vector3 mousePos;
    private Transform target;
    private Dictionary<SpawnEntry, List<Vector3>> predefinedPositions; // Vị trí đã tính từ warning

    public GameObject skillObjectPrefab;
    public float skyHeight = 200f;
    public int lineSpawnSpacing = 100;
    
    // Track tất cả skill object được spawn để có thể kiểm tra khi nào chúng explode
    private List<SkillObject> allSpawnedObjects = new List<SkillObject>();
    
    public List<SkillObject> GetAllSpawnedObjects()
    {
        // Lọc ra các object còn tồn tại
        allSpawnedObjects.RemoveAll(obj => obj == null);
        return new List<SkillObject>(allSpawnedObjects);
    }

    public void Init(
        SkillSpawnData spData,
        Vector3 casterPosition,
        Vector3 mousePosition,
        Transform targetFollow = null,
        Dictionary<SpawnEntry, List<Vector3>> spawnPositions = null)
    {
        spawnData = spData;

        casterPos = casterPosition;
        mousePos = mousePosition;
        target = targetFollow;
        predefinedPositions = spawnPositions; // Lưu vị trí từ warning

        transform.position = casterPosition;

        if (spawnData != null)
            StartCoroutine(SpawnEntriesSequentially());
    }


    // ===============================================================
    //                  1) CHAIN PER ENTRY
    // ===============================================================
    IEnumerator SpawnEntriesSequentially()
    {
        List<Coroutine> runningEntries = new List<Coroutine>();

        foreach (var entry in spawnData.spawnEntries)
        {
            if (entry.chainEntry)
            {
                // CHỜ entry này hoàn thành
                yield return StartCoroutine(SpawnEntryChain(entry));
            }
            else
            {
                // KHÔNG CHỜ – chạy song song
                Coroutine c = StartCoroutine(SpawnEntryChain(entry));
                runningEntries.Add(c);
            }
        }

        // CHỜ TẤT CẢ ENTRY SONG SONG HOÀN THÀNH
        foreach (var c in runningEntries)
            yield return c;

        // Đợi đến khi TẤT CẢ skill object explode/destroy trước khi destroy spawner
        yield return StartCoroutine(WaitForAllSpawnedObjectsDestroyed());

        Destroy(gameObject);
    }

    IEnumerator WaitForAllSpawnedObjectsDestroyed()
    {
        // Đợi đến khi tất cả skill object explode/destroy
        while (allSpawnedObjects.Count > 0)
        {
            // Lọc ra các object đã explode hoặc destroy
            allSpawnedObjects.RemoveAll(obj => obj == null || obj.gameObject == null || obj.exploded);
            yield return null;
        }
    }



    // ===============================================================
    //                  2) SPAWN ONE ENTRY
    // ===============================================================
    IEnumerator SpawnEntryChain(SpawnEntry entry)
    {
        Vector3 origin = GetOrigin(entry);

        // Nếu dùng burst
        if (entry.useBurst)
        {
            yield return StartCoroutine(SpawnWithBurst(entry, origin));
        }
        // Nếu dùng interval
        else if (entry.useInterval)
        {
            yield return StartCoroutine(SpawnWithInterval(entry, origin));
        }
        // Spawn tất cả ngay lập tức
        else
        {
            yield return StartCoroutine(SpawnAllAtOnce(entry, origin));
        }
    }
    IEnumerator SpawnAllAtOnce(SpawnEntry entry, Vector3 origin)
    {
        List<SkillObject> spawned = new List<SkillObject>();

        // Kiểm tra xem có vị trí đã tính sẵn từ warning không
        List<Vector3> positions = null;
        if (predefinedPositions != null && predefinedPositions.ContainsKey(entry))
        {
            positions = predefinedPositions[entry];
        }

        for (int i = 0; i < entry.count; i++)
        {
            // Sử dụng vị trí từ warning nếu có, nếu không thì tính toán
            Vector3 pos = (positions != null && i < positions.Count) 
                ? positions[i] 
                : CalculatePosition(i, entry, origin);
            bool explode = ShouldExplode(i, entry);

            var obj = CreateSkillInstance(entry.skillToSpawn, pos, explode, entry.movementType);
            spawned.Add(obj);
        }

        // chờ tất cả nổ
        yield return StartCoroutine(WaitForAllExplode(spawned));
    }
    IEnumerator SpawnWithInterval(SpawnEntry entry, Vector3 origin)
    {
        List<SkillObject> spawned = new List<SkillObject>();

        // Kiểm tra xem có vị trí đã tính sẵn từ warning không
        List<Vector3> positions = null;
        if (predefinedPositions != null && predefinedPositions.ContainsKey(entry))
        {
            positions = predefinedPositions[entry];
        }

        for (int i = 0; i < entry.count; i++)
        {
            Vector3 pos;
            if (positions != null && i < positions.Count)
            {
                // Sử dụng vị trí từ warning
                pos = positions[i];
            }
            else
            {
                // Cập nhật origin mỗi lần spawn và tính toán
                Vector3 currentOrigin = GetOrigin(entry);
                pos = CalculatePosition(i, entry, currentOrigin);
            }

            bool explode = ShouldExplode(i, entry);

            var obj = CreateSkillInstance(entry.skillToSpawn, pos, explode, entry.movementType);
            spawned.Add(obj);

            yield return new WaitForSeconds(entry.interval);
        }

        yield return StartCoroutine(WaitForAllExplode(spawned));
    }
    IEnumerator SpawnWithBurst(SpawnEntry entry, Vector3 origin)
    {
        List<SkillObject> spawned = new List<SkillObject>();

        // Kiểm tra xem có vị trí đã tính sẵn từ warning không
        List<Vector3> positions = null;
        if (predefinedPositions != null && predefinedPositions.ContainsKey(entry))
        {
            positions = predefinedPositions[entry];
        }

        int spawnedCount = 0;

        while (spawnedCount < entry.count)
        {
            int amount = Mathf.Min(entry.burstSize, entry.count - spawnedCount);

            // spawn 1 burst
            for (int i = 0; i < amount; i++)
            {
                int index = spawnedCount + i;

                Vector3 pos;
                if (positions != null && index < positions.Count)
                {
                    // Sử dụng vị trí từ warning
                    pos = positions[index];
                }
                else
                {
                    // Cập nhật origin mỗi burst (nếu origin lệ thuộc vào target)
                    Vector3 currentOrigin = GetOrigin(entry);
                    pos = CalculatePosition(index, entry, currentOrigin);
                }

                bool explode = ShouldExplode(index, entry);

                var obj = CreateSkillInstance(entry.skillToSpawn, pos, explode, entry.movementType);
                spawned.Add(obj);
            }

            spawnedCount += amount;

            // nếu còn burst tiếp theo → delay
            if (spawnedCount < entry.count)
                yield return new WaitForSeconds(entry.burstInterval);
        }

        yield return StartCoroutine(WaitForAllExplode(spawned));
    }


    IEnumerator WaitForAllExplode(List<SkillObject> objs)
    {
        HashSet<SkillObject> exploded = new HashSet<SkillObject>();

        foreach (var o in objs)
        {
            if (o.exploded)  
                exploded.Add(o);

            o.onExplode += (_) => exploded.Add(o);
        }

        // 3. Đợi đến khi exploded.Count == objs.Count
        yield return new WaitUntil(() => exploded.Count == objs.Count);
    }

    // ===============================================================
    //                EXPLOSION DECISION (CLEAN VERSION)
    // ===============================================================
    bool ShouldExplode(int index, SpawnEntry entry)
    {
        if (entry.allObjectsExplode) return true;
        return index == 0; // chỉ object đầu tiên được explode
    }


    // ===============================================================
    //                   PATTERN POSITION CALCULATION
    // ===============================================================
    Vector3 CalculatePosition(int index, SpawnEntry entry, Vector3 origin)
    {
        switch (entry.pattern)
        {
            case SpawnPattern.None:
                return origin;

            case SpawnPattern.Circle:
            case SpawnPattern.Radial:
                {
                    float step = 360f / entry.count;
                    float angle = (step * index + entry.angleOffset) * Mathf.Deg2Rad;

                    Vector3 offset = new Vector3(
                        Mathf.Cos(angle),
                        Mathf.Sin(angle),
                        0f
                    ) * entry.radius;

                    return origin + offset;
                }

            case SpawnPattern.RandomCircle:
                {
                    float angle = Random.Range(0f, Mathf.PI * 2f);
                    float r = Mathf.Sqrt(Random.value) * entry.radius;

                    Vector3 offset = new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r, 0);
                    return origin + offset;
                }
            case SpawnPattern.Line:
                {
                    Vector3 dir = (target.position - origin).normalized;
                    dir=new Vector3(dir.x, dir.y, 0f);

                    return origin + dir * lineSpawnSpacing * (index+1);
                }


        }

        return origin;
    }


    // ===============================================================
    //                   GET SPAWN ORIGIN
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
                return new Vector3(target.position.x, target.position.y + skyHeight, 0f);
        }
    }


    // ===============================================================
    //                   CREATE SKILL INSTANCE
    // ===============================================================
    SkillObject CreateSkillInstance(
     SkillObjectData data,
     Vector3 pos,
     bool isExplosive,
     SkillMovementType mtype,
     HashSet<SkillObject> exploded = null)
    {
        var obj = Instantiate(skillObjectPrefab, pos, Quaternion.identity);
        var sk = obj.GetComponent<SkillObject>();

        // REGISTER EVENT BEFORE PLAYING LOGIC
        if (exploded != null)
        {
            sk.onExplode += (_) =>
            {
                if (!exploded.Contains(sk))
                    exploded.Add(sk);
            };
        }

        // Đối với Projectile, không truyền target để nó sử dụng mousePos (vị trí đã khóa từ warning)
        // thay vì target.position (vị trí hiện tại có thể đã thay đổi)
        Transform targetToUse = (mtype == SkillMovementType.Projectile) ? null : target;
        sk.Init(data, casterPos, mousePos, isExplosive, mtype, targetToUse);
        
        // Thêm vào danh sách để track
        allSpawnedObjects.Add(sk);
        
        return sk;
    }

}
