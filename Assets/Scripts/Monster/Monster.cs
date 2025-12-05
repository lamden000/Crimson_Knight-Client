using UnityEngine;

public class Monster : BaseObject
{
    public MonsterTemplate Template;


    public static Monster Create(int id, short x, short y, int templateId)
    {
        GameObject gameObject = SpawnManager.GI().SpawnMonsterPrefab(x, y, templateId);
        Monster monster = gameObject.AddComponent<Monster>();
        monster.SetPosition(x, y);
        monster.Id = id;
        monster.Template = TemplateManager.MonsterTemplates[templateId];
        monster.Name = monster.Template.Name;
        return monster;
    }

    public override void AutoMoveToXY(int x, int y)
    {
    }

    public override void DestroyObject()
    {
    }
}
