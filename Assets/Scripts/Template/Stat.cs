public class Stat
{
    public StatId Id { get; set; }
    public int Value { get; set; }
    public Stat() { }
}

public enum StatId : byte
{
    HP = 0,
    MP = 1,
    ATK = 2,
    DEF = 3,

    PERCENT_HP = 4,
    PERCENT_MP = 5,
    PERCENT_ATK = 6,
    PERCENT_DEF = 7
}

public class StatDefinition
{
    public StatId StatId { get; set; }
    public string Name { get; set; }
    public bool IsPercent { get; set; }
}