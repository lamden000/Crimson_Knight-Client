using UnityEngine;

public enum MonsterState { Idle, Walk, Attack, GetHit }
//public enum MonsterName {Slime=1000,Snail=1001,Scorpion=1103,Bunny=1173,Frog=1215 }

public class MonsterPrefab : MonoBehaviour
{
    private MonsterMovementController movementController;
    //private MonsterName monsterName=MonsterName.Slime;

    public MonsterState currentState;
    private bool isDead=false;
    [Tooltip("Tự động tấn công người chơi khi lại gần")]
    public bool isHostile = false;

    public bool IsDead { get { return isDead; } }
    public int ImageId;
    void Start()
    {
        movementController = GetComponent<MonsterMovementController>();
        currentState = MonsterState.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //public MonsterName GetName()
    //{ return monsterName; }
    //public void SetName(MonsterName name)
    //{
    //    monsterName = name;
    //}

    public void SetState(MonsterState state)
    {
        if (isDead) return;
        currentState = state;
    }

    public void AniTakeDamage(GameObject attacker)
    {
        SetState(MonsterState.GetHit);
        if (movementController != null && attacker != null)
        {
            movementController.HandleGetHit(attacker.transform);
        }
    }
}
