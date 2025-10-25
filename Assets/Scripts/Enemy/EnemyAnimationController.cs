using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyAnimationController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private EnemyName _name;
    private EnemySpriteDatabase database;

    private float frameRate = 0.2f;
    private float timer;
    private int currentFrame;
    [SerializeField]
    private EnemyState currentState;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        _name = GetComponent<Enemy>().GetName();
        database=EnemySpriteDatabase.Instance;
        currentState = EnemyState.Idle;

        database.LoadSprites(_name);
    }

    // Update is called once per frame
    void Update()
    {
        PlayAnimation(currentState);
    }


    private void PlayAnimation(EnemyState state)
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
            frames = database.GetSprites(_name, EnemyState.Idle);
        }

        if (frames == null || frames.Count == 0)
            return;

        int frameIndex = currentFrame % frames.Count;
        spriteRenderer.sprite = frames[frameIndex];
    }
}
