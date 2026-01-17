using System.Collections.Generic;

public class MapTemplate
{
    public short Id { get; set; }
    public string Name { get; set; }
    public short XEnter { get; set; }
    public short YEnter { get; set; }
    public List<MonsterInMap> Monsters { get; set; }
    public List<NpcInMap> Npcs { get; set; }
    public bool IsPhoBan { get; set; }

    public class MonsterInMap
    {
        public int TemplateId { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
    }
    public class NpcInMap
    {
        public int TemplateId { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
    }
}