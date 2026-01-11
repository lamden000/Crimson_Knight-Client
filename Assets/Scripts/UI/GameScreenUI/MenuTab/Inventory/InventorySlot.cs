using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;

    private ItemData itemData;
    private int slotIndex;

    public void Init(int index)
    {
        slotIndex = index;
    }

    private void Awake()
    {
        if (iconImage == null)
            iconImage = GetComponentInChildren<Image>(true);

        Clear();
    }

    public void SetItem(ItemData data, Sprite sprite)
    {
        itemData = data;

        iconImage.sprite = sprite;
        iconImage.enabled = true;
        iconImage.raycastTarget = true;
        iconImage.color = Color.white;

        Debug.Log($"[INV][LOAD] Slot={slotIndex} name={data.name}");
    }

    public void Clear()
    {
        itemData = null;

        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
    }

    public ItemData GetItemData() => itemData;
    public Sprite GetSprite() => iconImage.sprite;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[INV][CLICK] Slot={slotIndex}");

        if (itemData == null)
        {
            InventoryManager.Instance.ClearInfo();
            return;
        }
        InventoryManager.Instance.ShowInfo(this);
    }
}
