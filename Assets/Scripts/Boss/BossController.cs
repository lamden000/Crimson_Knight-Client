using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BossController : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 3f;
    public float skillCheckInterval = 4f; // Kiểm tra skill mỗi 4s
    
    [Header("Normal Attack Settings")]
    public float attackRange = 1.5f; // Khoảng cách để đánh thường
    public float attackCooldown = 2f; // Cooldown giữa các lần đánh thường
    public float attackDamage = 20f; // Sát thương đánh thường
    
    [Header("References")]
    public Transform targetPlayer;
    public GameObject skillSpawnerPrefab; // Prefab chứa script SkillSpawnmer (có thể rỗng, sẽ tự tạo)
    private Canvas worldCanvas; // World Canvas để hiển thị warning UI (tự động tìm trong scene)

    [Header("Skills Configuration")]
    public List<BossSkillEntry> skills;

    private bool isCasting = false;
    private float skillTimer = 0f;
    private MonsterPrefab monsterPrefab; // Reference đến MonsterPrefab để quản lý state
    private float normalAttackCooldownTimer = 0f; // Timer cho cooldown đánh thường
    private bool isAttacking = false; // Đang trong quá trình đánh thường
    private TrailRenderer trailRenderer; // Reference đến TrailRenderer để bật khi dash
    private List<SkillObject> activeSkillObjects = new List<SkillObject>(); // Danh sách skill object đang active (nếu blockMovementWhileActive = true)

    [System.Serializable]
    public class BossSkillEntry
    {
        public string name;
        public SkillSpawnData spawnData;
        public float cooldownTime;
        [HideInInspector] public float lastUsedTime = -999f;

        [Header("Telegraph / Warning Area")]
        public float warningTime = 0f; // Thời gian hiện vùng đỏ trước khi skill active
        public GameObject warningUIPrefab; // Prefab UI warning riêng cho skill này (có Background và Fill Image)
        
        [Header("Skill Range & Dash")]
        public float attackRange = 5f; // Khoảng cách tối thiểu để cast skill này
        public bool useDash = false; // Có sử dụng dash trước khi cast skill không
        public float dashSpeed = 20f; // Tốc độ dash
        public float dashDistance = 2f; // Khoảng cách dash qua đằng sau target
        
        [Header("Movement Control")]
        public bool blockMovementWhileActive = false; // Nếu true, boss sẽ không di chuyển khi skill này đang active

        public bool IsReady()
        {
            return Time.time >= lastUsedTime + cooldownTime;
        }
    }

    void Start()
    {
        // Lấy MonsterPrefab component
        monsterPrefab = GetComponent<MonsterPrefab>();
        if (monsterPrefab == null)
        {
            Debug.LogWarning("BossController: MonsterPrefab component not found!");
        }

        // Lấy TrailRenderer component
        trailRenderer = GetComponent<TrailRenderer>();
        if (trailRenderer != null)
        {
            trailRenderer.enabled = false; // Tắt mặc định
        }

        // Tìm Canvas là child của Boss
        if (worldCanvas == null)
        {
            worldCanvas = GetComponentInChildren<Canvas>();
            if (worldCanvas == null)
            {
                Debug.LogWarning("BossController: Không tìm thấy Canvas là child của Boss!");
            }
            else
            {
                // Đảm bảo Canvas là World Space
                if (worldCanvas.renderMode != RenderMode.WorldSpace)
                {
                    worldCanvas.renderMode = RenderMode.WorldSpace;
                }
            }
        }

        // Nếu chưa gán player, thử tìm theo tag
        if (targetPlayer == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) targetPlayer = p.transform;
        }

        skillTimer = skillCheckInterval; // Đợi 4s đầu tiên rồi mới cast
    }

    void Update()
    {
        if (targetPlayer == null) return;

        // Update cooldown timer
        if (normalAttackCooldownTimer > 0)
        {
            normalAttackCooldownTimer -= Time.deltaTime;
        }

        // Kiểm tra nếu skill đang active và block movement
        // Lọc ra các object còn tồn tại
        activeSkillObjects.RemoveAll(obj => obj == null || obj.gameObject == null);
        bool isBlockedByActiveSkill = activeSkillObjects.Count > 0;

        // 1. Logic Timer
        if (!isCasting && !isAttacking && !isBlockedByActiveSkill)
        {
            skillTimer -= Time.deltaTime;
            
            // Di chuyển về phía player khi không cast, không đánh và không bị block bởi skill
            MoveTowardsPlayer();

            if (skillTimer <= 0)
            {
                bool skillCasted = TryCastRandomSkill();
                
                // Nếu không cast được skill (ngoài range hoặc cooldown), tiếp tục đuổi để đánh thường
                // MoveTowardsPlayer() đã xử lý việc đuổi và đánh thường
                
                skillTimer = skillCheckInterval; // Reset timer 4s
            }
        }
        else if (isBlockedByActiveSkill)
        {
            // Nếu bị block bởi skill, đứng yên
            if (monsterPrefab != null && monsterPrefab.currentState != MonsterState.Idle)
            {
                monsterPrefab.SetState(MonsterState.Idle);
            }
        }
    }

    void MoveTowardsPlayer()
    {
        float dist = Vector3.Distance(transform.position, targetPlayer.position);
        
        // Nếu đủ gần và cooldown hết thì đánh thường
        if (dist <= attackRange && normalAttackCooldownTimer <= 0 && !isAttacking)
        {
            StartCoroutine(NormalAttack());
        }
        else if (dist > attackRange)
        {
            // Di chuyển về phía player
            transform.position = Vector3.MoveTowards(transform.position, targetPlayer.position, moveSpeed * Time.deltaTime);
            
            // Set state thành Walk khi đang di chuyển
            if (monsterPrefab != null && monsterPrefab.currentState != MonsterState.Walk)
            {
                monsterPrefab.SetState(MonsterState.Walk);
            }
            
            // Flip Sprite logic và flip Canvas theo
            Vector3 newScale = transform.localScale;
            if (targetPlayer.position.x < transform.position.x) 
            {
                // Boss quay sang trái
                newScale.x = Mathf.Abs(newScale.x);
            }
            else 
            {
                // Boss quay sang phải
                newScale.x = -Mathf.Abs(newScale.x);
            }
            transform.localScale = newScale;
            
            // Flip Canvas theo Boss
            if (worldCanvas != null)
            {
                RectTransform canvasRect = worldCanvas.GetComponent<RectTransform>();
                if (canvasRect != null)
                {
                    // Flip Canvas scale X theo Boss
                    Vector3 canvasScale = canvasRect.localScale;
                    canvasScale.x = newScale.x < 0 ? -Mathf.Abs(canvasScale.x) : Mathf.Abs(canvasScale.x);
                    canvasRect.localScale = canvasScale;
                }
            }
        }
        else
        {
            // Đứng yên khi đang trong tầm đánh nhưng đang cooldown
            if (monsterPrefab != null && monsterPrefab.currentState != MonsterState.Idle)
            {
                monsterPrefab.SetState(MonsterState.Idle);
            }
        }
    }

    IEnumerator NormalAttack()
    {
        isAttacking = true;
        normalAttackCooldownTimer = attackCooldown;

        // Set state thành Attack
        if (monsterPrefab != null)
        {
            monsterPrefab.SetState(MonsterState.Attack);
        }

        // Đợi một chút để animation attack chạy
        yield return new WaitForSeconds(0.3f);

        // Gây damage cho player (nếu player vẫn trong tầm)
        float dist = Vector3.Distance(transform.position, targetPlayer.position);
        if (dist <= attackRange && targetPlayer != null)
        {
            // Tìm component Character để gây damage
            Character character = targetPlayer.GetComponent<Character>();
            if (character != null)
            {
                character.TakeDamage(attackDamage, gameObject);
            }
        }

        // Đợi thêm một chút để animation hoàn thành
        yield return new WaitForSeconds(0.1f);

        // Set state về Idle
        if (monsterPrefab != null)
        {
            monsterPrefab.SetState(MonsterState.Idle);
        }

        isAttacking = false;
    }

    bool TryCastRandomSkill()
    {
        if (targetPlayer == null) return false;

        float currentDistance = Vector3.Distance(transform.position, targetPlayer.position);

        // 1. Lọc ra các skill đang sẵn sàng VÀ trong range
        List<BossSkillEntry> readySkillsInRange = new List<BossSkillEntry>();
        foreach (var s in skills)
        {
            if (s.IsReady() && currentDistance <= s.attackRange)
            {
                readySkillsInRange.Add(s);
            }
        }

        // 2. Nếu có skill trong range -> Random chọn 1 và cast
        if (readySkillsInRange.Count > 0)
        {
            BossSkillEntry selected = readySkillsInRange[Random.Range(0, readySkillsInRange.Count)];
            StartCoroutine(CastSkillRoutine(selected));
            return true;
        }

        // 3. Nếu không có skill nào trong range hoặc đang cooldown -> Trả về false
        // Update() sẽ tiếp tục gọi MoveTowardsPlayer() để đuổi và đánh thường
        return false;
    }

    IEnumerator CastSkillRoutine(BossSkillEntry skillEntry)
    {
        isCasting = true;
        skillEntry.lastUsedTime = Time.time;


        // Lưu lại vị trí boss và target tại thời điểm bắt đầu để đảm bảo tính toán đúng
        Vector3 startBossPosition = transform.position;
        Vector3 lockedTargetPosition = targetPlayer.position;
        
        // Dash trước khi warning (nếu có)
        if (skillEntry.useDash && targetPlayer != null)
        {
            yield return StartCoroutine(DashToBehindTarget(skillEntry, lockedTargetPosition));
            
            // Dừng lại một nhịp sau khi dash xong để tránh lỗi
            yield return new WaitForSeconds(0.1f);
        }
        
        // Lưu lại vị trí boss sau khi dash (nếu có dash) để dùng cho spawn
        Vector3 bossPositionAfterDash = transform.position;
        
        // Hiển thị warning trước khi spawn skill - giữ animation Idle
        Dictionary<SpawnEntry, List<Vector3>> spawnPositions = null;
        if (skillEntry.warningTime > 0 && skillEntry.spawnData != null)
        {
            // Đảm bảo state là Idle khi đang warning
            if (monsterPrefab != null)
            {
                monsterPrefab.SetState(MonsterState.Idle);
            }
            
            spawnPositions = new Dictionary<SpawnEntry, List<Vector3>>();
            // Sử dụng vị trí boss sau dash để tính warning
            yield return StartCoroutine(ShowWarnings(skillEntry, bossPositionAfterDash, lockedTargetPosition, targetPlayer, spawnPositions));
        }

        // Set state thành Attack khi spawn skill
        if (monsterPrefab != null)
        {
            monsterPrefab.SetState(MonsterState.Attack);
        }

        // Tạo Spawner để bắn skill
        // Vì SkillSpawnmer tự hủy (Destroy) sau khi xong, ta nên Instantiate mới
        GameObject spawnerObj = null;
        if (skillSpawnerPrefab != null)
        {
            spawnerObj = Instantiate(skillSpawnerPrefab, bossPositionAfterDash, Quaternion.identity);
        }
        else
        {
            spawnerObj = new GameObject("BossSkillSpawner");
            spawnerObj.AddComponent<SkillSpawnmer>();
        }

        SkillSpawnmer spawner = spawnerObj.GetComponent<SkillSpawnmer>();
        
        // Init Spawner: Sử dụng bossPositionAfterDash và lockedTargetPosition
        // để skill spawn đúng vị trí và bắn đúng hướng đã warning
        spawner.Init(skillEntry.spawnData, bossPositionAfterDash, lockedTargetPosition, null, spawnPositions);

        // Giữ animation Attack trong 0.1 giây
        yield return new WaitForSeconds(0.1f); 

        // Set state về Idle sau khi animation attack xong
        if (monsterPrefab != null)
        {
            monsterPrefab.SetState(MonsterState.Idle);
        }

        isCasting = false;

        // Nếu skill này block movement, đợi đến khi spawner destroy (tức là tất cả skill object đã explode)
        if (skillEntry.blockMovementWhileActive)
        {
            // Lấy tất cả skill object từ spawner ngay sau khi spawn
            List<SkillObject> spawnedObjects = spawner.GetAllSpawnedObjects();
            activeSkillObjects.AddRange(spawnedObjects);
            
            // Đợi đến khi spawner destroy (tức là tất cả skill object đã explode/destroy)
            yield return new WaitUntil(() => spawner == null || spawner.gameObject == null);
            
            // Đảm bảo tất cả skill object đã explode/destroy
            activeSkillObjects.RemoveAll(obj => obj == null || obj.gameObject == null || obj.exploded);
            
            // Nếu vẫn còn object chưa explode, đợi thêm
            if (activeSkillObjects.Count > 0)
            {
                yield return new WaitUntil(() => 
                {
                    activeSkillObjects.RemoveAll(obj => obj == null || obj.gameObject == null || obj.exploded);
                    return activeSkillObjects.Count == 0;
                });
            }
            
            // Clear danh sách khi tất cả skill object đã explode
            activeSkillObjects.Clear();
        }
    }

    IEnumerator DashToBehindTarget(BossSkillEntry skillEntry, Vector3 targetPos)
    {
        // Tính toán vị trí đằng sau target
        Vector3 directionToTarget = (targetPos - transform.position).normalized;
        Vector3 dashDestination = targetPos - directionToTarget * skillEntry.dashDistance;
        dashDestination.z = transform.position.z; // Giữ nguyên Z

        // Bật TrailRenderer
        if (trailRenderer != null)
        {
            trailRenderer.enabled = true;
        }

        // Set state thành Walk khi dash
        if (monsterPrefab != null)
        {
            monsterPrefab.SetState(MonsterState.Walk);
        }

        // Dash đến vị trí đằng sau target
        while (Vector3.Distance(transform.position, dashDestination) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, dashDestination, skillEntry.dashSpeed * Time.deltaTime);
            yield return null;
        }

        // Đảm bảo đến đúng vị trí
        transform.position = dashDestination;

        // Tắt TrailRenderer
        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
        }

        // Set state về Idle sau khi dash xong
        if (monsterPrefab != null)
        {
            monsterPrefab.SetState(MonsterState.Idle);
        }
    }

    IEnumerator ShowWarnings(BossSkillEntry skillEntry, Vector3 casterPos, Vector3 targetPos, Transform target, Dictionary<SpawnEntry, List<Vector3>> spawnPositions)
    {
        if (worldCanvas == null)
        {
            Debug.LogWarning("World Canvas is not assigned!");
            yield break;
        }

        if (skillEntry.warningUIPrefab == null)
        {
            Debug.LogWarning($"Warning UI Prefab is not assigned for skill: {skillEntry.name}!");
            yield break;
        }

        List<GameObject> warningObjects = new List<GameObject>();

        // Tính toán tất cả vị trí spawn sẽ được tạo
        foreach (var entry in skillEntry.spawnData.spawnEntries)
        {
            Vector3 origin = GetSpawnOrigin(entry, casterPos, targetPos, target);
            List<Vector3> positions = new List<Vector3>();
            
            // Tính số lượng spawn
            int spawnCount = entry.count;
            if (entry.useBurst)
            {
                spawnCount = entry.count; // Tổng số vẫn là entry.count
            }

            // Tạo warning cho mỗi vị trí spawn và lưu lại vị trí
            for (int i = 0; i < spawnCount; i++)
            {
                Vector3 spawnPos = CalculateSpawnPosition(i, entry, origin, targetPos, target);
                positions.Add(spawnPos);
                
                GameObject warningObj = CreateWarningIndicator(skillEntry, spawnPos, entry, casterPos, targetPos, target);
                if (warningObj != null)
                    warningObjects.Add(warningObj);
            }

            spawnPositions[entry] = positions;
        }

        // Chạy animation fill cho tất cả warnings
        float timer = 0f;
        while (timer < skillEntry.warningTime)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / skillEntry.warningTime);

            // Cập nhật fill cho tất cả warnings
            foreach (var warningObj in warningObjects)
            {
                UpdateWarningFill(warningObj, t);
            }

            yield return null;
        }

        // Đảm bảo fill đầy 100%
        foreach (var warningObj in warningObjects)
        {
            UpdateWarningFill(warningObj, 1f);
        }

        // Xóa tất cả warnings
        foreach (var warningObj in warningObjects)
        {
            if (warningObj != null)
                Destroy(warningObj);
        }
    }

    GameObject CreateWarningIndicator(BossSkillEntry skillEntry, Vector3 worldPosition, SpawnEntry entry, Vector3 casterPos, Vector3 targetPos, Transform target)
    {
        // Instantiate warning UI prefab từ skillEntry
        GameObject warningObj = Instantiate(skillEntry.warningUIPrefab, worldCanvas.transform);
        
        warningObj.SetActive(true);
        // Set position trong world space (Canvas phải là World Space)
        RectTransform rectTransform = warningObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.position = worldPosition;

            // Nếu là skill projectile, xoay warning theo hướng từ vị trí warning đến target đã khóa
            if (entry.movementType == SkillMovementType.Projectile)
            {
                // Luôn dùng targetPos (lockedTargetPosition) thay vì target.position để đảm bảo hướng đúng
                Vector3 direction = targetPos - worldPosition;
                direction.z = 0f; // Đảm bảo chỉ xoay trên trục Z
                
                if (direction.magnitude > 0.01f)
                {
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    // SkillObject.Projectile() xoay với angle + 90f (sprite nhìn xuống)
                    // Warning có pivot ở bên trái và hướng sang phải mặc định
                    // Vì pivot ở bên trái, cần xoay 180 độ để hướng đúng về phía target
                    rectTransform.rotation = Quaternion.Euler(0, 0, angle );
                }
            }
        }

        // Đảm bảo fill image bắt đầu từ 0
        Image fillImage = warningObj.transform.Find("Fill")?.GetComponent<Image>();
        if (fillImage != null)
        {
            fillImage.fillAmount = 0f;
            // Set fill method là Radial 360 để fill từ trong ra ngoài
            fillImage.type = Image.Type.Filled;
            fillImage.fillOrigin = 2; // Center
        }

        return warningObj;
    }

    void UpdateWarningFill(GameObject warningObj, float fillAmount)
    {
        if (warningObj == null) return;

        // Tìm Fill Image và cập nhật fillAmount
        Image fillImage = warningObj.transform.Find("Fill")?.GetComponent<Image>();
        if (fillImage != null)
        {
            fillImage.fillAmount = fillAmount;
        }
    }

    Vector3 GetSpawnOrigin(SpawnEntry entry, Vector3 casterPos, Vector3 targetPos, Transform target)
    {
        switch (entry.origin)
        {
            case SpawnOrigin.CasterPosition:
                return casterPos;

            case SpawnOrigin.TargetPosition:
                return target ? target.position : targetPos;

            case SpawnOrigin.MousePosition:
                return targetPos;

            default:
            case SpawnOrigin.Sky:
                float skyHeight = 200f; // Cùng giá trị với SkillSpawnmer
                return new Vector3(targetPos.x, targetPos.y + skyHeight, 0f);
        }
    }

    Vector3 CalculateSpawnPosition(int index, SpawnEntry entry, Vector3 origin, Vector3 targetPos, Transform target)
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
                    // Sử dụng index làm seed để có vị trí cố định
                    Random.State oldState = Random.state;
                    Random.InitState(index);
                    float angle = Random.Range(0f, Mathf.PI * 2f);
                    float r = Mathf.Sqrt(Random.value) * entry.radius;
                    Random.state = oldState;

                    Vector3 offset = new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r, 0);
                    return origin + offset;
                }

            case SpawnPattern.Line:
                {
                    Vector3 dir = (target ? target.position : targetPos) - origin;
                    dir = new Vector3(dir.x, dir.y, 0f).normalized;
                    int lineSpawnSpacing = 100; // Cùng giá trị với SkillSpawnmer
                    return origin + dir * lineSpawnSpacing * (index + 1);
                }
        }

        return origin;
    }
}

