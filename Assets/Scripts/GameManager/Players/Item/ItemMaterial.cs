public class ItemMaterial : BaseItem
{
    public ItemMaterial(int templateId, int quantity)
    {
        this.TemplateId = templateId;
        this.Id = templateId.ToString();
        this.Quantity = quantity;
    }
    public int Quantity { get; set; }

    public override ItemType GetItemType()
    {
        return ItemType.Material;
    }
}