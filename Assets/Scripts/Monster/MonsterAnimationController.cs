using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Monster))]
public class MonsterAnimationController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private MonsterName _name;
    private MonsterSpriteDatabase database;
    private Monster enemy;

    private float frameRate = 0.2f;
    private float timer;
    private int currentFrame;


    void Start()
    {
        enemy = GetComponent<Monster>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        _name = enemy.GetName();
        boxCollider = GetComponent<BoxCollider2D>();
        database=MonsterSpriteDatabase.Instance;

        database.LoadSprites(_name);

        spriteRenderer.sprite = database.GetSprites(_name,enemy.currentState)[0];
        AdjustColliderToSprite();
    }

    public void AdjustColliderToSprite()
    {
        if (spriteRenderer.sprite == null)
        {
            Debug.LogWarning("Không có Sprite để điều chỉnh Collider.");
            return;
        }

        Bounds spriteBounds = spriteRenderer.sprite.bounds;

        boxCollider.size = spriteBounds.size;

        boxCollider.offset = spriteBounds.center;

        Debug.Log($"Collider đã được điều chỉnh. Kích thước mới: {boxCollider.size}, Offset: {boxCollider.offset}");
    }

    void Update()
    {
        PlayAnimation(enemy.currentState);
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

        frames = database.GetSprites(_name,state);

        if ((frames == null || frames.Count == 0))
        {
            frames = database.GetSprites(_name, MonsterState.Idle);
        }
        else
        {
            int frameIndex = currentFrame % frames.Count;
            spriteRenderer.sprite = frames[frameIndex];
        }
    }

}
