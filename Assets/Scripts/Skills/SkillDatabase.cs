using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skill/SkillDatabase")]
public class SkillDatabase : ScriptableObject
{
    public List<SkillData> allSkills;

    public SkillData GetSkillByName(SkillName name)
    {
        return allSkills.Find(s => s.skillName == name);
    }

    public SkillData GetSkillByID(int id)
    {
        if (id < 0 || id >= allSkills.Count) return null;
        return allSkills[id];
    }
}
