
using System.Collections.Generic;
using UnityEngine;

public enum SpawnPattern
{
    None,
    Radial,
    Fan,
    RandomCircle,
    Circle
}

public enum SpawnOrigin
{
    Sky,  
    CasterPosition,
    TargetPosition,
    MousePosition
}


[CreateAssetMenu(menuName = "Skill/SkillData")]
public class SkillData : ScriptableObject
{
    public List<SpawnEntry> spawnEntries;
}

[System.Serializable]
public class SpawnEntry
{
    public SkillObjectData skillToSpawn;

    public SpawnPattern pattern = SpawnPattern.None;
    public int count = 1;
    public float radius = 1f;
    public float angleOffset = 0f;

    public SpawnOrigin origin = SpawnOrigin.CasterPosition;
    public bool allObjectExplode=true;
}
