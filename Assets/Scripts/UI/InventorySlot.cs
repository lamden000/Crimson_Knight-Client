using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    public ItemData item; 
    public Image icon;

    public void SetItem(ItemData newItem)
    {
        item = newItem;

        if (item == null)
        {
            icon.enabled = false;
            icon.sprite = null;
            return;
        }

        icon.enabled = true;
        icon.sprite = InventoryManager.Instance.LoadIconById(item.spriteId);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (item == null)
        {
            Debug.Log("Slot trống");
            InventoryManager.Instance.ClearInfo();
        }
        else
        {
            Debug.Log($"Click item → ItemID = {item.itemId}, SpriteID = {item.spriteId}");
            InventoryManager.Instance.ShowInfo(item);
        }
    }
}
