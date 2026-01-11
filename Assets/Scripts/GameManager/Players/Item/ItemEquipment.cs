public class ItemEquipment : BaseItem
{
    public ItemEquipment(string id, int templateId)
    {
        this.Id = id;
        this.TemplateId = templateId;
    }
    public override ItemType GetItemType()
    {
        return ItemType.Equipment;
    }
}