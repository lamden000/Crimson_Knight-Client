using Assets.Scripts.Networking;
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
        monster.Level = monster.Template.Level;
        GameObject nameTag = SpawnManager.GI().SpawnDisplayBaseObjectNamePrefab(monster.Name);
        nameTag.transform.SetParent(monster.transform);
        nameTag.transform.localPosition = new Vector3(0, monster.GetTopOffsetY(), 0);
        monster.SetNameTag(nameTag);
        return monster;
    }

    public override void AutoMoveToXY(int x, int y)
    {
    }


    public override ObjectType GetObjectType()
    {
        return ObjectType.Monster;
    }

    public void LoadBaseInfoFromServer(Message msg)
    {
        this.CurrentHp = msg.ReadInt();
        this.MaxHp = msg.ReadInt();
    }
}
