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

[CreateAssetMenu(menuName = "Skill/SkillObjectData")]
public class SkillObjectData : ScriptableObject
{
    public SkillName skillName;

    [Header("Main Animation Frames")]
    public bool mainLoop = true;
    public Sprite[] mainFrames;
    public float mainFPS = 12f;

    [Header("Optional Sparkle Frame Animation")]
    public Sprite[] sparkleFrames;
    public float sparkleFPS = 12f;

    [Header("Aftermath Effect Frames")]
    public Sprite[] aftermathFrames;
    public float aftermathFPS = 12f;
    public bool explosionRotatesWithMovement = true;

    [Header("Movement")]
    public SkillMovementType movementType;
    public float autoExplosionTime = 5f;
    public float speed = 100f;
    public float explosionDelay = 0f;

}
