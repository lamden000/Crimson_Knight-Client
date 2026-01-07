using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Npc : BaseObject
{
    public NpcTemplate Template;
    public static Npc Create(short x, short y, int templateId)
    {
        GameObject gameObject = SpawnManager.GI().SpawnNpcPrefab(x, y, templateId);
        Npc npc = gameObject.AddComponent<Npc>();
        npc.SetPosition(x, y);
        npc.Id = templateId;
        npc.Template = TemplateManager.NpcTemplates[templateId];
        npc.Name = npc.Template.Name;
        return npc;
    }


    public override void AutoMoveToXY(int x, int y)
    {
    }
   
    public override ObjectType GetObjectType()
    {
        return ObjectType.Npc;
    }
}
