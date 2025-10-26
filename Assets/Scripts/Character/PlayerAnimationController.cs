using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PlayerAnimationController;

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

    private Direction currentDir;
    private State currentState;

    private CharacterSpriteDatabase database;
    Character character;

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
        database = CharacterSpriteDatabase.Instance;
        character = gameObject.GetComponent<Character>();
        currentDir=Direction.Down;
        currentState = State.Idle;

        weaponType = character.getWeaponType();
        partVariants[weaponType]=weaponVariant;
        spriteRenderers[weaponType]=weaponSpriteRenderer;


        LoadSprites();
    }

    private void Update()
    {
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
            if (dir == Direction.Up)
                SetDirectionUp(true);
            else
            {
                SetDirectionUp(false);
            }

            currentDir = dir;
            currentState = state;
            currentFrame = 0;
            timer = 0;
        }
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
            spriteRenderers[weaponType].sortingOrder = hairOrder+1;
            spriteRenderers[CharacterPart.Wings].sortingOrder = hairOrder + 2;
        }
        else
        {
            spriteRenderers[CharacterPart.Eyes].gameObject.SetActive(true);
            int legOrder = spriteRenderers[CharacterPart.Legs].sortingOrder;
            spriteRenderers[weaponType].sortingOrder = legOrder - 1;
            spriteRenderers[CharacterPart.Wings].sortingOrder = legOrder - 2;
        }

    }

    public void SetAttackAnimation(bool isAttacking)
    {
        if(isAttacking)
        {
            spriteRenderers[weaponType].gameObject.SetActive(false);
            currentEyeState=EyeState.Attack;
            StartCoroutine(ResetAttackAnimation(0.3f));
        } 
        else
        {
            spriteRenderers[weaponType].gameObject.SetActive(true);
            currentEyeState = EyeState.Idle;
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

}
