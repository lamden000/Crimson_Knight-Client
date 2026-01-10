using System.Collections.Generic;

public class ItemEquipmentTemplate : ItemTemplateBase
{
    public Dictionary<StatId, Stat> Stats { get; set; }

    public Gender Gender { get; set; }

    public ClassType ClassType { get; set; }

    public EquipmentType EquipmentType { get; set; }
    public int PartId { get; set; }

    public override ItemType GetItemType()
    {
        return ItemType.Equipment;
    }
}