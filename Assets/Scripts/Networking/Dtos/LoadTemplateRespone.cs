using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Networking.Dtos
{
    public class LoadTemplateRespone
    {
        public List<MonsterTemplate> MonsterTemplates;
        public List<NpcTemplate> NpcTemplates;
        public Dictionary<ClassType, List<SkillTemplate>> SkillTemplates;
        public List<ItemEquipmentTemplate> ItemEquipmentTemplates;
        public List<ItemConsumableTemplate> ItemConsumableTemplates;
        public List<ItemMaterialTemplate> ItemMaterialTemplates;
        public Dictionary<StatId, StatDefinition> StatDefinitions;
        public List<int> Levels;
        public List<QuestTemplate> QuestTemplates;
    }
}
