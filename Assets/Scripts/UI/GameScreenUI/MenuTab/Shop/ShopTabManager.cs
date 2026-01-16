using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Assets.Scripts;

public class ShopTabManager : MonoBehaviour
{
    public static ShopTabManager Instance;

    [Header("Slots")]
    public Transform slotParent;
    public ShopItemSlot slotPrefab;

    [Header("Info Panel")]
    public Image infoIconCur;
    public TextMeshProUGUI infoNameCur;
    public TextMeshProUGUI infoDescriptionCur;

    [Header("Info Panel Buttons")]
    public TextMeshProUGUI useButtonText;

    private List<ShopItemSlot> slots = new List<ShopItemSlot>();

    private void Awake()
    {
        Instance = this;
    }

    private static bool isInit = false;
    private static readonly int SIZE_INVEN = 48;

    private void OnEnable()
    {
        InitSlots();
        LoadFromPlayerInventory();
    }

    private void InitSlots()
    {
        if (isInit) return;
        isInit = true;

        slots.Clear();
        for (int i = 0; i < SIZE_INVEN; i++)
        {
            ShopItemSlot slot = Instantiate(slotPrefab, slotParent);
            slots.Add(slot);
        }

        ClearInfoCur();
    }

    public void LoadFromPlayerInventory()
    {
        if (ClientReceiveMessageHandler.Player == null ||
            ClientReceiveMessageHandler.Player.InventoryItems == null)
            return;

        foreach (var slot in slots)
            slot.Clear();

        ClearInfoCur();

        BaseItem[] items = ClientReceiveMessageHandler.Player.InventoryItems;

        for (int i = 0; i < items.Length && i < slots.Count; i++)
        {
            if (items[i] != null)
                SetItemToSlot(i, items[i]);
        }
    }

    private void SetItemToSlot(int slotIndex, BaseItem item)
    {
        Sprite sprite = GetSprite(item.GetIcon(), item.GetItemType());
        if (sprite == null) return;

        slots[slotIndex].SetItem(item, sprite);
    }

    private Sprite GetSprite(int iconId, ItemType type)
    {
        Sprite sprite = null;
        switch (type)
        {
            case ItemType.Equipment:
                ResourceManager.ItemEquipmentIconSprites.TryGetValue(iconId, out sprite);
                break;
            case ItemType.Consumable:
                ResourceManager.ItemConsumableIconSprites.TryGetValue(iconId, out sprite);
                break;
            case ItemType.Material:
                ResourceManager.ItemMaterialsIconSprites.TryGetValue(iconId, out sprite);
                break;
        }
        return sprite;
    }

    private ShopItemSlot selectedSlot;

    public void SelectSlot(ShopItemSlot slot)
    {
        if (selectedSlot != null)
            selectedSlot.SetSelected(false);

        selectedSlot = slot;
        selectedSlot.SetSelected(true);

        ShowInfo(slot);
    }

    public void ShowInfo(ShopItemSlot slot)
    {
        if (slot == null || slot.Item == null)
        {
            ClearInfoCur();
            return;
        }

        BaseItem item = slot.Item;
        infoIconCur.enabled = true;
        infoIconCur.sprite = slot.GetSprite();

        infoNameCur.text = item.GetName();
        infoDescriptionCur.text =
            $"Cấp yêu cầu: {item.GetLevelRequired()}\n{item.GetDescription()}";

        UpdateUseButtonText(item);
    }

    private void UpdateUseButtonText(BaseItem item)
    {
        if (item == null)
        {
            useButtonText.text = "";
            return;
        }

        switch (item.GetItemType())
        {
            case ItemType.Equipment:
                useButtonText.text = "Mua / Trang bị";
                break;
            default:
                useButtonText.text = "Mua";
                break;
        }
    }

    public void ClearInfoCur()
    {
        if (selectedSlot != null)
        {
            selectedSlot.SetSelected(false);
            selectedSlot = null;
        }

        infoIconCur.enabled = false;
        infoIconCur.sprite = null;
        infoNameCur.text = "";
        infoDescriptionCur.text = "";
    }
}
