using UnityEngine;
using UnityEngine.UI;

public class CharacterEquipSlot : MonoBehaviour
{
    public EquipmentType equipType;
    public Image icon;

    public void Clear()
    {
        icon.sprite = null;
        icon.enabled = false;
    }

    public void SetItem(ItemEquipment item)
    {
        if (item == null)
        {
            Clear();
            return;
        }

        if (Assets.Scripts.ResourceManager.ItemEquipmentIconSprites
            .TryGetValue(item.TemplateId, out Sprite sp))
        {
            icon.sprite = sp;
            icon.enabled = true;
            icon.color = Color.white;
        }
        else
        {
            Debug.LogWarning($"[CHAR EQUIP] Missing icon template={item.TemplateId}");
            Clear();
        }
    }
}
