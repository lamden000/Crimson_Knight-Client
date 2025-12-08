using Unity.VisualScripting;
using UnityEngine;

public enum SkillName { ScorchingStrike, GammaMeteor, Vortex,DeadlyCircle }
public enum SkillMovementType
{
    Projectile,        
    Homing,       
    ProjectileFromSky, 
    PersistentArea,   
    Wave,        
    Orbit      
}

public enum SkillSpawnPoint
{
    CasterPosition,
    TargetPosition,
    Sky,
    MousePosition
}

public enum SpawnPattern
{
    None,
    Radial,
    Fan,
    RandomCircle,
    Circle
}

[CreateAssetMenu(menuName = "Skill/SkillData")]
public class SkillData : ScriptableObject
{
    public SkillName skillName;
    public SkillSpawnPoint spawnPoint;

    [Header("Main Animation Frames")]
    public bool mainLoop = true;
    public Sprite[] mainFrames;
    public float mainFPS = 12f;

    [Header("Optional Sparkle Frame Animation")]
    public Sprite[] sparkleFrames;
    public float sparkleFPS = 12f;
    public bool hasSparkle;

    [Header("Aftermath Effect Frames")]
    public Sprite[] aftermathFrames;
    public float aftermathFPS = 12f;
    public bool hasAftermath;
    public bool explosionRotatesWithMovement = true;

    [Header("Movement")]
    public SkillMovementType movementType;
    public float autoExplosionTime = 5f;
    public float speed = 100f;
    public float explosionDelay = 0f;

    [Header("Spawn Multiple Settings (Optional)")]
    public bool spawnMultiple = false;
    public bool cloneExplodes = true; // clone có nổ không
    public int spawnCount = 1;       // số viên sinh ra
    public float spawnRadius = 0;    // bán kính spawn
    public SpawnPattern spawnPattern;   // radial / fan / random / none

}
