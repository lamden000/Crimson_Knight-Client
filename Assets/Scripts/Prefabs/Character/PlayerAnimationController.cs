using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [System.Serializable]
    public struct PartRendererPair
    {
        public CharacterPart part;
        public SpriteRenderer renderer;
    }

    [System.Serializable]
    public struct PartVariantPair
    {
        public CharacterPart part;
        public int variant;
    }

    [SerializeField] private float frameRate = 0.2f;

    [SerializeField]
    private List<PartRendererPair> spriteRenderersInspector;
    private Dictionary<CharacterPart, SpriteRenderer> spriteRenderers;

    [SerializeField]
    private List<PartVariantPair> variantsInspector;

    [SerializeField]
    private EyeState currentEyeState;
    [SerializeField]
    private SpriteRenderer weaponSpriteRenderer;
    [SerializeField]
    int weaponVariant = 0;

    private CharacterPart weaponType;

    private Dictionary<CharacterPart, int> partVariants;

    private float timer;
    private int currentFrame;
    private float blinkTimer = 0f;
    private float blinkDuration = 0.2f;
    private float blinkInterval = 2f;

    public Direction currentDir;
    public State currentState;

    private CharacterSpriteDatabase database;
    public GameObject minimapIndicator;
    public AttackAnimationController attackAnimation;
    Character character;

    // Lưu trạng thái renderer trước khi chết để khôi phục sau này
    private Dictionary<CharacterPart, bool> savedRendererStates = new Dictionary<CharacterPart, bool>();
    private bool isDead = false;

    public Direction GetCurrentDirection()
    { return currentDir; }

    private void Awake()
    {
        spriteRenderers = new Dictionary<CharacterPart, SpriteRenderer>();
        foreach (var pair in spriteRenderersInspector)
        {
            spriteRenderers[pair.part] = pair.renderer;
        }

        partVariants = new Dictionary<CharacterPart, int>();
        foreach (var pair in variantsInspector)
        {
            partVariants[pair.part] = pair.variant;
        }
    }


    private void Start()
    {
        
    }

    public void SetUp()
    {
        database = CharacterSpriteDatabase.Instance;
        character = gameObject.GetComponent<Character>();
        currentDir = Direction.Down;
        currentState = State.Idle;

        weaponType = character.getWeaponType();
        if (attackAnimation != null)
        {
            attackAnimation.SetWeaponType(weaponType);
        }

        partVariants[weaponType] = weaponVariant;
        spriteRenderers[weaponType] = weaponSpriteRenderer;

        LoadSprites();
    }

    private void LateUpdate()
    {
        // Không cập nhật animation nếu đã chết
        if (isDead) return;

        PlayAnimation(currentDir, currentState,currentEyeState);
        Blink();
    }

    private void LoadSprites()
    {
        foreach (var kvp in partVariants.ToList()) 
        {
            LoadPart(kvp.Key, kvp.Value);
        }
    }

    public void SetAnimation(Direction dir, State state)
    {
        if (dir != currentDir || state != currentState)
        {

            if (dir == Direction.Up) SetDirectionUp(true);

            else SetDirectionUp(false);

            transform.rotation = (dir == Direction.Right) ? Quaternion.Euler(0, 180f, 0) : Quaternion.identity;

            currentDir = dir;
            UpdateMinimapIndicator(dir);

            if (state == State.Attack)
                SetAttackAnimation(true);
            else if (currentState==State.Attack)
            {
                SetAttackAnimation(false);
            }
            currentState = state;
            currentFrame = 0;
            timer = 0;
        }
    }

    void UpdateMinimapIndicator(Direction dir)
    {
        if (minimapIndicator == null) return;

        float angle = 0f;

        switch (dir)
        {
            case Direction.Down:
                angle = 0f;
                break;
            case Direction.Left:
                angle = 270f;
                break;
            case Direction.Up:
                angle = 180f;
                break;
            case Direction.Right:
                angle = 270f;
                break;
        }

        minimapIndicator.transform.localRotation = Quaternion.Euler(0, 0, angle);
    }


    private void PlayAnimation(Direction dir, State state, EyeState eyeState)
    {
        if (database == null) return; 

        timer += Time.deltaTime;
        if (timer >= frameRate)
        {
            timer = 0f;
            currentFrame++;
        }

        foreach (var part in spriteRenderers.Keys)
        {
            List<Sprite> frames = null;
            if(dir == Direction.Right)
                dir = Direction.Left;

            if (part != CharacterPart.Eyes)
            {
                frames = database.GetSprites(dir, state, partVariants[part], part);

                if ((frames == null || frames.Count == 0))
                {
                    frames = database.GetSprites(dir, State.Idle, partVariants[part], part);
                }
            }
            else
            {
                frames = database.GetEyeSprites(partVariants[part],dir, eyeState);
            }

            if (frames == null || frames.Count == 0) continue;

            int frameIndex = currentFrame % frames.Count;
            spriteRenderers[part].sprite = frames[frameIndex];
        }
    }

    private void Blink()
    {
        if(currentEyeState!=EyeState.Idle)
            return;
        blinkTimer += Time.deltaTime;
        if (blinkTimer >= blinkInterval)
        {
            blinkTimer = 0f;
        }

        float t = blinkTimer / blinkDuration;
        if (t <= 1f)
        {
            float scaleY = 1f - Mathf.Abs(Mathf.Lerp(-1f, 1f, t));
            var eyesGO = spriteRenderers[CharacterPart.Eyes].gameObject;
            Vector3 s = eyesGO.transform.localScale;
            s.y = Mathf.Max(0f, scaleY); // tránh âm
            eyesGO.transform.localScale = s;
        }
        else
        {
            var eyesGO = spriteRenderers[CharacterPart.Eyes].gameObject;
            Vector3 s = eyesGO.transform.localScale;
            s.y = 1f;
            eyesGO.transform.localScale = s;
        }
    }

    public void SetDirectionUp(bool isUp)
    {
        if (isUp)
        {
            spriteRenderers[CharacterPart.Eyes].gameObject.SetActive(false);
            int hairOrder = spriteRenderers[CharacterPart.Hair].sortingOrder;
            spriteRenderers[weaponType].sortingOrder = hairOrder;
            spriteRenderers[CharacterPart.Wings].sortingOrder = hairOrder + 1;
        }
        else
        {
            spriteRenderers[CharacterPart.Eyes].gameObject.SetActive(true);
            int legOrder = spriteRenderers[CharacterPart.Legs].sortingOrder;
            spriteRenderers[weaponType].sortingOrder = legOrder - 1;
            spriteRenderers[CharacterPart.Wings].sortingOrder = legOrder - 2;
        }

    }

    private void SetAttackAnimation(bool isAttacking)
    {
        if (attackAnimation == null)
        {
            return;
        }
        if(isAttacking)
        {
            spriteRenderers[weaponType].gameObject.SetActive(false);
            currentEyeState=EyeState.Attack;

            attackAnimation.gameObject.SetActive(true);
            attackAnimation.PlayAttackAnimation(currentDir);

            StartCoroutine(ResetAttackAnimation(0.5f));
           // StartCoroutine(ResetAttackAnimation(2));
        } 
        else
        {
            attackAnimation.gameObject.SetActive(false);
            spriteRenderers[weaponType].gameObject.SetActive(true);
            currentEyeState = EyeState.Idle;
            PlayerMovementController movementController = GetComponent<PlayerMovementController>();

            if (movementController != null)
            {
                if (movementController.rb != null && movementController.rb.linearVelocity.magnitude > 0.1f)
                {
                    currentState = State.Walk;
                }
                else
                {
                    currentState = State.Idle; 
                }
            }
            else
            {
                currentState = State.Idle;
            }
        }
    }

    public void SetGetHitAnimation(bool getHit)
    {
        if (getHit)
        {
            currentEyeState = EyeState.GetHit;
        }
        else
        {
            currentEyeState = EyeState.Idle;
        }

    }

    private IEnumerator ResetAttackAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetAttackAnimation(false);
    }


    public void LoadPartVuKhi(ClassType classType, int variant)
    {
        if (classType == ClassType.CHIEN_BINH)
        {
            this.LoadPart(CharacterPart.Sword, variant);
        }
        else if (classType == ClassType.SAT_THU)
        {
            this.LoadPart(CharacterPart.Knive, variant);
        }
        else if (classType == ClassType.PHAP_SU)
        {
            this.LoadPart(CharacterPart.Staff, variant);
        }
        else
        {
            this.LoadPart(CharacterPart.Gun, variant);
        }
    }


    public void LoadPart(CharacterPart part, int variant)
    {
        partVariants[part] = variant;
        switch (part)
        {
            case CharacterPart.Body:
                spriteRenderers[CharacterPart.Body].sprite = null;
                database.LoadBody(variant);
                break;
            case CharacterPart.Legs:
                spriteRenderers[CharacterPart.Legs].sprite = null;
                database.LoadLegs(variant);
                break;
            case CharacterPart.Head:
                spriteRenderers[CharacterPart.Head].sprite = null;
                database.LoadHead(variant);
                break;
            case CharacterPart.Hair:
                spriteRenderers[CharacterPart.Hair].sprite = null;
                database.LoadHair(variant);
                break;
            case CharacterPart.Hat:
                spriteRenderers[CharacterPart.Hat].sprite = null;
                database.LoadHat(variant);
                break;
            case CharacterPart.Sword:
                spriteRenderers[weaponType].sprite = null;
                database.LoadWeapon(variant,character.getWeaponType());
                break;
            case CharacterPart.Gun:
                spriteRenderers[weaponType].sprite = null;
                database.LoadWeapon(variant, character.getWeaponType());
                break;
            case CharacterPart.Knive:
                spriteRenderers[weaponType].sprite = null;
                database.LoadWeapon(variant, character.getWeaponType());
                break;
            case CharacterPart.Staff:
                spriteRenderers[weaponType].sprite = null;
                database.LoadWeapon(variant, character.getWeaponType());
                break;
            case CharacterPart.Wings:
                spriteRenderers[CharacterPart.Wings].sprite = null;
                database.LoadWings(variant);
                break;
            case CharacterPart.Eyes:
                spriteRenderers[CharacterPart.Eyes].sprite = null;
                database.LoadEyes(variant);
                break;
        }
    }

    // Set trạng thái chết
    public void SetDeadState()
    {
        if (isDead) return; // Đã chết rồi thì không làm gì

        isDead = true;
        
        // Lưu trạng thái enabled của các renderer (trừ Eyes và Body)
        savedRendererStates.Clear();
        foreach (var part in spriteRenderers.Keys)
        {
            if (part != CharacterPart.Eyes && part != CharacterPart.Body && spriteRenderers.ContainsKey(part))
            {
                savedRendererStates[part] = spriteRenderers[part].enabled;
                spriteRenderers[part].enabled = false; // Tắt renderer
            }
        }

        // Tắt attack animation nếu đang bật
        if (attackAnimation != null)
        {
            attackAnimation.gameObject.SetActive(false);
        }

        // Set dead sprite vào body
        Sprite deadSprite = database.GetDeadBodySprite();
        if (deadSprite != null && spriteRenderers.ContainsKey(CharacterPart.Body))
        {
            spriteRenderers[CharacterPart.Body].sprite = deadSprite;
        }

        // Set eye state = Beaten
        currentEyeState = EyeState.Beaten;
        // Cập nhật mắt ngay lập tức
        if (spriteRenderers.ContainsKey(CharacterPart.Eyes))
        {
            var eyeFrames = database.GetEyeSprites(partVariants[CharacterPart.Eyes], currentDir, EyeState.Beaten);
            if (eyeFrames != null && eyeFrames.Count > 0)
            {
                spriteRenderers[CharacterPart.Eyes].sprite = eyeFrames[0];
            }
        }

        // Set state = Dead
        currentState = State.Dead;
    }

    // Khôi phục trạng thái sống
    public void SetAliveState()
    {
        if (!isDead) return; // Đang sống rồi thì không làm gì

        isDead = false;

        // Khôi phục trạng thái enabled của các renderer
        foreach (var kvp in savedRendererStates)
        {
            if (spriteRenderers.ContainsKey(kvp.Key))
            {
                spriteRenderers[kvp.Key].enabled = kvp.Value;
            }
        }
        savedRendererStates.Clear();

        // Set eye state = Idle
        currentEyeState = EyeState.Idle;

        // Set state = Idle
        currentState = State.Idle;

        // Load lại sprites bình thường (chỉ cần đảm bảo body được load lại)
        if (spriteRenderers.ContainsKey(CharacterPart.Body))
        {
            spriteRenderers[CharacterPart.Body].sprite = null; // Reset để PlayAnimation sẽ load lại
        }
    }

}
