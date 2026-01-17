using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopItemSlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI quantityText;

    public BaseItem Item;
    public int Price;

    private void Awake()
    {
        if (iconImage == null)
            iconImage = GetComponentInChildren<Image>(true);

        Clear();
    }

    public void SetItem(BaseItem data, Sprite sprite, int price)
    {
        Item = data;
        this.Price = price;
        iconImage.sprite = sprite;
        iconImage.enabled = true;
        iconImage.raycastTarget = true;
        iconImage.color = Color.white;

        int quantity = 1;
        if (Item.GetItemType() == ItemType.Consumable)
            quantity = ((ItemConsumable)Item).Quantity;
        else if (Item.GetItemType() == ItemType.Material)
            quantity = ((ItemMaterial)Item).Quantity;

        if (quantityText != null)
            quantityText.text = quantity > 1 ? quantity.ToString() : "";
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
            ShopTabManager.Instance.ClearInfoCur();
            return;
        }

        ShopTabManager.Instance.SelectSlot(this);
    }

    public void SetSelected(bool selected)
    {
        if (backgroundImage != null)
        {
            Color color;
            if (selected)
                ColorUtility.TryParseHtmlString("#7D5745", out color);
            else
                ColorUtility.TryParseHtmlString("#A87A64", out color);

            backgroundImage.color = color;
        }
    }
}
