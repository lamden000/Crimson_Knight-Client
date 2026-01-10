public class ItemConsumableTemplate : ItemTemplateBase
{
    public int Cooldown { get; set; }
    public long Value { get; set; }
    public override ItemType GetItemType()
    {
        return ItemType.Consumable;
    }
}