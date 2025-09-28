using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.TextCore.Text;

public class AnimationController : MonoBehaviour
{
    public string folderPath = "Character Sprites";
    [SerializeField] private float frameRate = 0.2f;

    [System.Serializable]
    public struct PartRendererPair
    {
        public CharacterPart part;
        public SpriteRenderer renderer;
    }

    [SerializeField]
    private List<PartRendererPair> spriteRenderersInspector;
    private Dictionary<CharacterPart, SpriteRenderer> spriteRenderers;
    private Character character;

    private Dictionary<Direction,Dictionary<State,Dictionary<CharacterPart, List<Sprite>>>> database = new Dictionary<Direction,Dictionary<State, Dictionary<CharacterPart, List<Sprite>>>>();

    [Header ("Variant")]
    public int outfitVariant = 0;
    public int hairVariant = 0;
    public int headVariant = 0;
    public int hatVariant = 0;
    public int eyeVariant = 9;
    public int wingVariant = 0;
    public int weaponVariant = 1;

    private Vector3 defaultEyeOffset;
    private Vector3 defaultWingOffset;

    private float timer;
    private int currentFrame;
    private float blinkTimer = 0f;
    private float blinkDuration = 0.2f;
    private float blinkInterval = 2f;  

    Direction currentDir;
    State currentState;

    private void Awake()
    {
        spriteRenderers = new Dictionary<CharacterPart, SpriteRenderer>();
        foreach (var pair in spriteRenderersInspector)
        {
            spriteRenderers[pair.part] = pair.renderer;
        }
    }


    private void Start()
    {
        currentDir=Direction.Down;
        currentState = State.Idle;
        defaultEyeOffset = spriteRenderers[CharacterPart.Eyes].transform.localPosition;
        defaultWingOffset = spriteRenderers[CharacterPart.Wings].transform.localPosition;

        character= GetComponent<Character>();
        foreach (var kvp in spriteRenderers)
        {
            if (kvp.Value == null)
                Debug.LogError($"SpriteRenderer for {kvp.Key} is null!");
        }
        LoadSprites();
    }

    private void Update()
    {
        PlayAnimation(currentDir, currentState);
        Blink();
    }

    private void PlayAnimation(Direction dir, State state)
    {
        if (!database.ContainsKey(dir)) return;
        if (!database[dir].ContainsKey(state)) return;

        timer += Time.deltaTime;
        if (timer >= frameRate)
        {
            timer = 0f;
            currentFrame++;
        }

        foreach (var part in spriteRenderers.Keys)
        {
            List<Sprite> frames = null;

            if (database.ContainsKey(dir) &&
                database[dir].ContainsKey(state) &&
                database[dir][state].ContainsKey(part))
            {
                frames = database[dir][state][part];
            }

            if ((frames == null || frames.Count == 0) &&
                database.ContainsKey(dir) &&
                database[dir].ContainsKey(State.Idle) &&
                database[dir][State.Idle].ContainsKey(part))
            {
                frames = database[dir][State.Idle][part];
            }

            if (frames == null || frames.Count == 0) continue;

            int frameIndex = currentFrame % frames.Count;
            spriteRenderers[part].sprite = frames[frameIndex];
        }
    }

    private void Blink()
    {
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
            spriteRenderers[CharacterPart.Sword].sortingOrder = 4;
            spriteRenderers[CharacterPart.Wings].sortingOrder = 5;
        }
        else
        {
            spriteRenderers[CharacterPart.Eyes].gameObject.SetActive(true);
            spriteRenderers[CharacterPart.Sword].sortingOrder = -1;
            spriteRenderers[CharacterPart.Wings].sortingOrder = -2;
        }

    }

    /// <summary>
    /// Dịch mắt theo trục X (lệch trái/phải)
    /// </summary>
    public void SetLeftOffset()
    {
        var eye = spriteRenderers[CharacterPart.Eyes].transform;
        var wings = spriteRenderers[CharacterPart.Wings].transform;
        wings.localPosition = new Vector3(0.24f, defaultWingOffset.y, 0);
        eye.localPosition = new Vector3(-0.06f, defaultEyeOffset.y, 0);
    }

    /// <summary>
    /// Reset mắt về vị trí gốc
    /// </summary>
    public void ResetLeftOffset()
    {
        var eye = spriteRenderers[CharacterPart.Eyes].transform;
        eye.localPosition = defaultEyeOffset;
        var wings = spriteRenderers[CharacterPart.Wings].transform;
        wings.localPosition = defaultWingOffset;
    }

    public void SetAnimation(Direction dir, State state)
    {
        if (dir != currentDir || state != currentState)
        {
            if(dir==Direction.Up)
                SetDirectionUp(true);
            else
            {
                SetDirectionUp(false);
            }
            if (dir == Direction.Left || dir == Direction.Right)
            {
                SetLeftOffset();
            }
            else
            {
                ResetLeftOffset();
            }

            currentDir = dir;
            currentState = state;
            currentFrame = 0;
            timer = 0;
        }
    }

    private void LoadSprites()
    {
        LoadBodyAndLegs();
        LoadHead();
        LoadHair();
        LoadEyes();
        LoadHat();
        LoadWings();
        LoadWeapon();
    }

    private void EnsureDatabase(Direction dir, State state, CharacterPart part)
    {
        if (!database.ContainsKey(dir))
            database[dir] = new Dictionary<State, Dictionary<CharacterPart, List<Sprite>>>();

        if (!database[dir].ContainsKey(state))
            database[dir][state] = new Dictionary<CharacterPart, List<Sprite>>();

        if (!database[dir][state].ContainsKey(part))
            database[dir][state][part] = new List<Sprite>();
    }

    private void ClearPartSprites(CharacterPart part)
    {
        foreach (var dir in database.Keys)
        {
            foreach (var state in database[dir].Keys)
            {
                if (database[dir][state].ContainsKey(part))
                    database[dir][state][part].Clear();
            }
        }
    }

    private void AddSprite(CharacterPart part, int variant, int stateCode, Direction dir, State state)
    {
        EnsureDatabase(dir, state, part);

        string sheetName = $"{(int)part}_{variant}";
        string spriteName = $"{(int)part}_{variant}_{stateCode}";

        Sprite[] all = Resources.LoadAll<Sprite>(folderPath + "/" + sheetName);
        Sprite sprite = System.Array.Find(all, s => s.name == spriteName);

        if (sprite != null)
            database[dir][state][part].Add(sprite);
        else
            Debug.LogWarning($"Sprite not found: {spriteName}");
    }

    public void SetVariant(CharacterPart part, int newVariant)
    {
        // load lại tùy theo part
        switch (part)
        {
            case CharacterPart.Body:
                outfitVariant = newVariant;
                LoadBodyAndLegs();
                break;
            case CharacterPart.Legs:
                outfitVariant = newVariant;
                LoadBodyAndLegs();
                break;
            case CharacterPart.Head:
                headVariant = newVariant;
                LoadHead();
                break;
            case CharacterPart.Hair:
                hairVariant = newVariant;
                LoadHair();
                break;
            case CharacterPart.Hat:
                hatVariant = newVariant;
                LoadHat();
                break;
            case CharacterPart.Sword:
                weaponVariant = newVariant;
                LoadWeapon();
                break;
            case CharacterPart.Wings:
                wingVariant = newVariant;
                LoadWings();
                break;
        }
    }


    public void LoadBodyAndLegs()
    {
        if (outfitVariant == -1)
            return;
        ClearPartSprites(CharacterPart.Body);
        ClearPartSprites(CharacterPart.Legs);
        AddSprite(CharacterPart.Body, outfitVariant, (int)BodyState.IdleDown, Direction.Down, State.Idle);
        AddSprite(CharacterPart.Body, outfitVariant, (int)BodyState.IdleUp, Direction.Up, State.Idle);
        AddSprite(CharacterPart.Body, outfitVariant, (int)BodyState.IdleLeft, Direction.Left, State.Idle);

        AddSprite(CharacterPart.Body, outfitVariant, (int)BodyState.WalkDown_1, Direction.Down, State.Walk);
        AddSprite(CharacterPart.Body, outfitVariant, (int)BodyState.WalkDown_2, Direction.Down, State.Walk);
        AddSprite(CharacterPart.Body, outfitVariant, (int)BodyState.WalkUp_1, Direction.Up, State.Walk);
        AddSprite(CharacterPart.Body, outfitVariant, (int)BodyState.WalkUp_2, Direction.Up, State.Walk);
        AddSprite(CharacterPart.Body, outfitVariant, (int)BodyState.WalkLeft_1, Direction.Left, State.Walk);
        AddSprite(CharacterPart.Body, outfitVariant, (int)BodyState.WalkLeft_2, Direction.Left, State.Walk);

        AddSprite(CharacterPart.Body, outfitVariant, (int)BodyState.AttackDown, Direction.Down, State.Attack);
        AddSprite(CharacterPart.Body, outfitVariant, (int)BodyState.AttackUp, Direction.Up, State.Attack);
        AddSprite(CharacterPart.Body, outfitVariant, (int)BodyState.AttackLeft, Direction.Left, State.Attack);

        AddSprite(CharacterPart.Legs, outfitVariant, (int)BodyState.IdleDown, Direction.Down, State.Idle);
        AddSprite(CharacterPart.Legs, outfitVariant, (int)BodyState.IdleUp, Direction.Up, State.Idle);
        AddSprite(CharacterPart.Legs, outfitVariant, (int)BodyState.IdleLeft, Direction.Left, State.Idle);

        AddSprite(CharacterPart.Legs, outfitVariant, (int)BodyState.WalkDown_1, Direction.Down, State.Walk);
        AddSprite(CharacterPart.Legs, outfitVariant, (int)BodyState.WalkDown_2, Direction.Down, State.Walk);
        AddSprite(CharacterPart.Legs, outfitVariant, (int)BodyState.WalkUp_1, Direction.Up, State.Walk);
        AddSprite(CharacterPart.Legs, outfitVariant, (int)BodyState.WalkUp_2, Direction.Up, State.Walk);
        AddSprite(CharacterPart.Legs, outfitVariant, (int)BodyState.WalkLeft_1, Direction.Left, State.Walk);
        AddSprite(CharacterPart.Legs, outfitVariant, (int)BodyState.WalkLeft_2, Direction.Left, State.Walk);

        AddSprite(CharacterPart.Legs, outfitVariant, (int)BodyState.AttackDown, Direction.Down, State.Attack);
        AddSprite(CharacterPart.Legs, outfitVariant, (int)BodyState.AttackUp, Direction.Up, State.Attack);
        AddSprite(CharacterPart.Legs, outfitVariant, (int)BodyState.AttackLeft, Direction.Left, State.Attack);
    }
    public void LoadHead()
    {
        if (headVariant == -1)
            return;
        ClearPartSprites(CharacterPart.Head);
        AddSprite(CharacterPart.Head, headVariant, (int)HeadState.Down, Direction.Down, State.Idle);
        AddSprite(CharacterPart.Head, headVariant, (int)HeadState.Up, Direction.Up, State.Idle);
        AddSprite(CharacterPart.Head, headVariant, (int)HeadState.Left, Direction.Left, State.Idle);
    }

    public void LoadHat()
    {
        ClearPartSprites(CharacterPart.Hat);
        spriteRenderers[CharacterPart.Hat].sprite = null;
        if (hatVariant == -1)
            return;
        AddSprite(CharacterPart.Hat, hatVariant, (int)HeadState.Down, Direction.Down, State.Idle);
        AddSprite(CharacterPart.Hat, hatVariant, (int)HeadState.Up, Direction.Up, State.Idle);
        AddSprite(CharacterPart.Hat, hatVariant, (int)HeadState.Left, Direction.Left, State.Idle);
    }

    public void LoadHair()
    {
        if (hairVariant == -1)
            return;
        ClearPartSprites(CharacterPart.Hair);
        AddSprite(CharacterPart.Hair, hairVariant, (int)HeadState.Down, Direction.Down, State.Idle);
        AddSprite(CharacterPart.Hair, hairVariant, (int)HeadState.Up, Direction.Up, State.Idle);
        AddSprite(CharacterPart.Hair, hairVariant, (int)HeadState.Left, Direction.Left, State.Idle);
    }

    public void LoadEyes()
    {
        if (eyeVariant == -1)
            return;
        ClearPartSprites(CharacterPart.Eyes);
        AddSprite(CharacterPart.Eyes, eyeVariant, (int)HeadState.Down, Direction.Down, State.Idle);
        AddSprite(CharacterPart.Eyes, eyeVariant, (int)HeadState.Up, Direction.Up, State.Idle);
        AddSprite(CharacterPart.Eyes, eyeVariant, (int)HeadState.Left, Direction.Left, State.Idle);
    }

    public void LoadWings()
    {
        ClearPartSprites(CharacterPart.Wings);
        spriteRenderers[CharacterPart.Wings].sprite=null;
        if (wingVariant==-1)
            return;
        AddSprite(CharacterPart.Wings, wingVariant, (int)WingState.Down_1, Direction.Down, State.Idle);
        AddSprite(CharacterPart.Wings, wingVariant, (int)WingState.Down_2, Direction.Down, State.Idle);

        AddSprite(CharacterPart.Wings, wingVariant, (int)WingState.Down_1, Direction.Up, State.Idle);
        AddSprite(CharacterPart.Wings, wingVariant, (int)WingState.Down_2, Direction.Up, State.Idle);


        AddSprite(CharacterPart.Wings, wingVariant, (int)WingState.Left_1, Direction.Left, State.Idle);
        AddSprite(CharacterPart.Wings, wingVariant, (int)WingState.Left_2, Direction.Left, State.Idle);
    }

    public void LoadWeapon()
    {
        CharacterPart weapon=CharacterPart.Sword;
        Character.Class charClass=character.GetClass();
        switch (charClass){
            case Character.Class.Assassin:
                weapon = CharacterPart.Knive;
                break;
            case Character.Class.Knight:
                weapon = CharacterPart.Sword;
                break;
            case Character.Class.Wizard:
                weapon = CharacterPart.Staff;
                break;
            case Character.Class.Markman:
                weapon = CharacterPart.Gun;
                break;
        }

        ClearPartSprites(weapon);
        spriteRenderers[weapon].sprite = null;
        if (weaponVariant == -1)
            return;
        AddSprite(weapon, weaponVariant, (int)WeaponState.Down, Direction.Down, State.Idle);
        AddSprite(weapon, weaponVariant, (int)WeaponState.Up, Direction.Up, State.Idle);
        AddSprite(weapon, weaponVariant, (int)WeaponState.Left, Direction.Left, State.Idle);
    }
}
