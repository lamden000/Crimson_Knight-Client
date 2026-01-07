using UnityEngine;

public class NPCAnimatonController : MonoBehaviour
{
    [Header("Animation Settings")]
    public float frameRate = 1f; // seconds per frame
    public bool playOnStart = true;

    private SpriteRenderer spriteRenderer;
    private Sprite[] idleSprites;
    private int currentFrame;
    private float timer;
    private bool isPlaying;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (playOnStart)
            PlayIdle();
    }

    private void Update()
    {
        if (!isPlaying || idleSprites == null || idleSprites.Length == 0)
            return;

        timer += Time.deltaTime;
        if (timer >= frameRate)
        {
            timer = 0f;
            currentFrame = (currentFrame + 1) % idleSprites.Length;
            spriteRenderer.sprite = idleSprites[currentFrame];
        }
    }

    /// <summary>
    /// Loads the Idle sprites from Resources/NPCs/Sprites/{index}.png
    /// </summary>
    public void LoadIdleSprites(NpcTemplate template)
    {
        int index = (int)template.ImageId;
        string path = $"NPCs/Sprites/{index}";
        idleSprites = Resources.LoadAll<Sprite>(path);

        if (idleSprites == null || idleSprites.Length == 0)
        {
            Debug.LogWarning($"No sprites found at path: Resources/{path}");
            return;
        }

        // Set first frame
        spriteRenderer.sprite = idleSprites[0];
    }

    /// <summary>
    /// Start playing Idle animation
    /// </summary>
    public void PlayIdle()
    {
        if (idleSprites == null || idleSprites.Length == 0)
            return;

        isPlaying = true;
        currentFrame = 0;
        spriteRenderer.sprite = idleSprites[0];
    }

    /// <summary>
    /// Stop the animation (keep current frame)
    /// </summary>
    public void Stop()
    {
        isPlaying = false;
    }

    /// <summary>
    /// Force a specific frame index
    /// </summary>
    public void SetFrame(int frame)
    {
        if (idleSprites == null || frame < 0 || frame >= idleSprites.Length)
            return;

        currentFrame = frame;
        spriteRenderer.sprite = idleSprites[currentFrame];
    }
}
