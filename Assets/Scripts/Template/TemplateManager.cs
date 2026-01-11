using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TemplateManager
{
    public static List<MonsterTemplate> MonsterTemplates = new List<MonsterTemplate>();
    public static List<NpcTemplate> NpcTemplates = new List<NpcTemplate>();
    public static Dictionary<ClassType, List<SkillTemplate>> SkillTemplates = new Dictionary<ClassType, List<SkillTemplate>>();
    public static List<ItemEquipmentTemplate> ItemEquipmentTemplates = new List<ItemEquipmentTemplate>();
    public static List<ItemConsumableTemplate> ItemConsumableTemplates = new List<ItemConsumableTemplate>();
    public static List<ItemMaterialTemplate> ItemMaterialTemplates = new List<ItemMaterialTemplate>();
    public static Dictionary<StatId, StatDefinition> StatDefinitions = new Dictionary<StatId, StatDefinition>();
}
