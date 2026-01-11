using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage; 

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
        if (Item == null)
        {
            InventoryManager.Instance.ClearInfoCur();
            return;
        }

        InventoryManager.Instance.SelectSlot(this);
    }

    public void SetSelected(bool selected)
    {
        if (backgroundImage != null)
        {
            Color color;
            if (selected)
            {
                ColorUtility.TryParseHtmlString("#7D5745", out color);
            }
            else
            {
                // Nhạt hơn khi không chọn (ví dụ xanh nhạt)
                ColorUtility.TryParseHtmlString("#A87A64", out color);
            }
            backgroundImage.color = color;
        }
    }

}
