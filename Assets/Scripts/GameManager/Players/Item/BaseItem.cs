public abstract class BaseItem
{
    public string Id { get; set; }
    public int TemplateId { get; set; }
    public abstract ItemType GetItemType();
}