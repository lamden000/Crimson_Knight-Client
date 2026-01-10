using UnityEngine;

public enum MonsterState { Idle, Walk, Attack, GetHit }

public class MonsterPrefab : MonoBehaviour
{
    private MonsterMovementController movementController;

    public MonsterState currentState;
    private bool isDead=false;


    public int ImageId;
    void Start()
    {
        movementController = GetComponent<MonsterMovementController>();
        currentState = MonsterState.Idle;
    }

    public void SetState(MonsterState state)
    {
        if (isDead) return;
        currentState = state;
    }

    public void AniTakeDamage()
    {
        SetState(MonsterState.GetHit);
        if (movementController != null)
        {
            movementController.HandleGetHit();
        }
    }

    public void AniAttack(GameObject target)
    {
        SetState(MonsterState.Attack);

        if (movementController != null && target != null)
        {
            bool shouldFlipRight = target.transform.position.x > transform.position.x;
            movementController.FlipSprite(shouldFlipRight);
        }

        StartCoroutine(ResetToIdleAfterAttack());
    }
    private System.Collections.IEnumerator ResetToIdleAfterAttack()
    {
        float delay = movementController != null ? movementController.animationDelay : 0.4f;
        yield return new WaitForSeconds(delay);

        SetState(MonsterState.Idle);
    }
}
