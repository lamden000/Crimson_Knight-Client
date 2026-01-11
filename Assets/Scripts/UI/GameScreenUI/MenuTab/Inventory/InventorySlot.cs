using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;

    public BaseItem Item;

 
    private void Awake()
    {
        if (iconImage == null)
            iconImage = GetComponentInChildren<Image>(true);

        Clear();
    }

    public void SetItem(BaseItem data, Sprite sprite)
    {
        Item = data;

        iconImage.sprite = sprite;
        iconImage.enabled = true;
        iconImage.raycastTarget = true;
        iconImage.color = Color.white;
    }

    public void Clear()
    {
        Item = null;
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
    }

    public Sprite GetSprite() => iconImage.sprite;

    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log($"[INV][CLICK] Slot={slotIndex}");

        //if (itemData == null)
        //{
        //    InventoryManager.Instance.ClearInfo();
        //    return;
        //}
        InventoryManager.Instance.ShowInfo(this);
    }
}
