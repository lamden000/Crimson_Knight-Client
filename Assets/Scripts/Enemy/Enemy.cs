using UnityEngine;

public enum EnemyState { Idle, Walk, Attack, GetHit }
public enum EnemyName {Slime=1000,Snail=1001,Scorpion=1103 }

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private EnemyName _name=EnemyName.Slime;
    private bool isDead;

    public bool IsDead { get { return isDead; } }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public EnemyName GetName()
    { return _name; }
}
