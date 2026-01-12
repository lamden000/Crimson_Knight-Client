using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.GameManager.Map
{
    public class ItemPick : BaseObject
    {
        public string IdItemPick { get; set; }
        public static ItemPick Create(string id,int templateId, ItemType itemType, short x, short y)
        {
            x += (short)MathUtil.RandomInt(-50, 50);
            y += (short)MathUtil.RandomInt(-50, 50);

            GameObject obj = SpawnManager.GI().SpawnPickItem(templateId, itemType, new Vector2(x, y));
            ItemPick itemPick = obj.AddComponent<ItemPick>();
            itemPick.IdItemPick = id;
            itemPick.SetPosition(x, y);
            string name = "";
            short lv = 0;
            if (itemType == ItemType.Equipment)
            {
                name = TemplateManager.ItemEquipmentTemplates[templateId].Name;
                lv = TemplateManager.ItemEquipmentTemplates[templateId].LevelRequire;
            }
            else if (itemType == ItemType.Consumable)
            {
                name = TemplateManager.ItemConsumableTemplates[templateId].Name;
                lv = TemplateManager.ItemConsumableTemplates[templateId].LevelRequire;
            }
            else
            {
                name = TemplateManager.ItemMaterialTemplates[templateId].Name;
                lv = TemplateManager.ItemConsumableTemplates[templateId].LevelRequire;
            }
            itemPick.Level = lv;
            itemPick.Name = name;
            itemPick.CurrentHp = 1;
            itemPick.MaxHp = 1;
            GameObject nameTag = SpawnManager.GI().SpawnDisplayBaseObjectNamePrefab(itemPick.Name);
            nameTag.transform.SetParent(itemPick.transform);
            nameTag.transform.localPosition = new Vector3(0, itemPick.GetTopOffsetY(), 0);
            itemPick.SetNameTag(nameTag);
            return itemPick;
        }
        public override void AutoMoveToXY(int x, int y)
        {
        }

        public override bool IsItemPick()
        {
            return true;
        }
    }
}
