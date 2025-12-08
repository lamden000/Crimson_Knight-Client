[System.Serializable]
public class SkillData
{
    public int id;              
    public string name;
    public string description;

    public int mana;
    public float cooldown;
    public float damage;

    public int spriteId;        

    public int level = 1;
    public int maxLevel = 5;

    public SkillData(
        int id,
        string name,
        string description,
        int manaCost,
        float cooldown,
        float damage,
        int spriteId
    )
    {
        this.id = id;
        this.name = name;
        this.description = description;

        this.mana = manaCost;
        this.cooldown = cooldown;
        this.damage = damage;

        this.spriteId = spriteId;
    }
}
