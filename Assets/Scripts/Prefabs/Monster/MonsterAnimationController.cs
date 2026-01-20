using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MonsterPrefab))]
public class MonsterAnimationController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private MonsterSpriteDatabase database;
    private MonsterPrefab monster;

    private float frameRate = 0.2f;
    private float timer;
    private int currentFrame;


    void Start()
    {
        monster = GetComponent<MonsterPrefab>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        database=MonsterSpriteDatabase.Instance;

        database.LoadSprites(monster.ImageId);

        AdjustColliderToSprite();
    }

    public void AdjustColliderToSprite()
    {
        if (spriteRenderer.sprite == null)
        {
            Debug.LogWarning("Không có Sprite để điều chỉnh Collider.");
            return;
        }
        Sprite idleSprite = database.GetSprites(monster.ImageId, MonsterState.Idle)[0];
        Bounds spriteBounds = idleSprite.bounds;

        // Đặt size: giữ nguyên x, giảm y xuống 0.3
        boxCollider.size = new Vector2(spriteBounds.size.x*0.5f,spriteBounds.size.y* 0.3f);

        // Dịch collider xuống đáy của sprite
        // Offset y = min.y của sprite + một nửa chiều cao collider (0.15)
        // Offset x giữ nguyên ở center của sprite
        float colliderCenterY = spriteBounds.min.y + boxCollider.size.y/2;
        boxCollider.offset = new Vector2(spriteBounds.center.x, colliderCenterY);
    }

    void Update()
    {
        PlayAnimation(monster.currentState);
    }


    private void PlayAnimation(MonsterState state)
    {
        if (database == null) return;

        timer += Time.deltaTime;
        if (timer >= frameRate)
        {
            timer = 0f;
            currentFrame++;
        }

        List<Sprite> frames = null;

        frames = database.GetSprites(monster.ImageId, state);

        if ((frames == null || frames.Count == 0))
        {
            frames = database.GetSprites(monster.ImageId, MonsterState.Idle);
        }
        else
        {
            int frameIndex = currentFrame % frames.Count;
            spriteRenderer.sprite = frames[frameIndex];
        }
    }

}
