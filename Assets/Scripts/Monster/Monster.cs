using UnityEngine;

public enum EnemyState { Idle, Walk, Attack, GetHit }
public enum EnemyName {Slime=1000,Snail=1001,Scorpion=1103,Bunny=1173,Frog=1215 }

public class Monster : MonoBehaviour
{
    private MonsterMovementController movementController;
    [SerializeField]
    private EnemyName _name=EnemyName.Slime;

    public EnemyState currentState;
    private bool isDead=false;
    [Tooltip("Tự động tấn công người chơi khi lại gần")]
    public bool isHostile = false;

    public bool IsDead { get { return isDead; } }
    void Start()
    {
        movementController = GetComponent<MonsterMovementController>();
        currentState = EnemyState.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public EnemyName GetName()
    { return _name; }

    public void SetState(EnemyState state)
    {
        if (isDead) return;
        currentState = state;
    }

    public void TakeDamage(int damage,GameObject attacker)
    {
        SetState(EnemyState.GetHit);
        if (movementController != null && attacker != null)
        {
            movementController.HandleGetHit(attacker.transform);
        }
    }
}
