public abstract class BaseItem
{
    public string Id { get; set; }
    public int TemplateId { get; set; }
    public abstract ItemType GetItemType();

    public string GetItemId() { return Id; }
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

    public string GetName()
    {
        if (GetItemType() == ItemType.Equipment)
        {
            return TemplateManager.ItemEquipmentTemplates[TemplateId].Name;
        }

        if (GetItemType() == ItemType.Consumable)
        {
            return TemplateManager.ItemConsumableTemplates[TemplateId].Name;
        }

        return TemplateManager.ItemMaterialTemplates[TemplateId].Name;
    }

    public string GetDescription()
    {
        if (GetItemType() == ItemType.Equipment)
        {
            return TemplateManager.ItemEquipmentTemplates[TemplateId].Description;
        }

        if (GetItemType() == ItemType.Consumable)
        {
            return TemplateManager.ItemConsumableTemplates[TemplateId].Description;
        }

        return TemplateManager.ItemMaterialTemplates[TemplateId].Description;
    }
    public int GetLevelRequired()
    {
        if (GetItemType() == ItemType.Equipment)
        {
            return TemplateManager.ItemEquipmentTemplates[TemplateId].LevelRequire;
        }

        if (GetItemType() == ItemType.Consumable)
        {
            return TemplateManager.ItemConsumableTemplates[TemplateId].LevelRequire;
        }

        return TemplateManager.ItemMaterialTemplates[TemplateId].LevelRequire;
    }

    public int GetQuantity()
    {
        if (GetItemType() == ItemType.Consumable)
        {
            return ((ItemConsumable)this).Quantity;
        }
        else if (GetItemType() == ItemType.Material)
        {
            return ((ItemMaterial)this).Quantity;
        }
        return 1;

    }
}