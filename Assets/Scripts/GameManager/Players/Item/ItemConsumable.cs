public class ItemConsumable : BaseItem
{
    public ItemConsumable(int templateId, int quantity)
    {
        this.TemplateId = templateId;
        this.Id = templateId.ToString();
        this.Quantity = quantity;
    }
    public int Quantity { get; set; }
    public override ItemType GetItemType()
    {
        return ItemType.Consumable;
    }

}