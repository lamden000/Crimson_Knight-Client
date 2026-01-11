public abstract class BaseItem
{
    public string Id { get; set; }
    public int TemplateId { get; set; }
    public abstract ItemType GetItemType();

    public int GetIcon()
    {
        if (GetItemType() == ItemType.Equipment)
        {
            return TemplateManager.ItemEquipmentTemplates[TemplateId].IconId;
        }

        if(GetItemType() == ItemType.Consumable)
        {
            return TemplateManager.ItemConsumableTemplates[TemplateId].IconId;
        }

        return TemplateManager.ItemMaterialTemplates[TemplateId].IconId;
    }
}