public abstract class ItemTemplateBase
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public short LevelRequire { get; set; }
    public int IconId { get; set; }
    public abstract ItemType GetItemType();
}