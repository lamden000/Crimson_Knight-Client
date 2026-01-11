using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Assets.Scripts;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Slots")]
    public Transform slotParent;
    public InventorySlot slotPrefab;

    [Header("Info Panel")]
    public Image infoIconCur;
    public TextMeshProUGUI infoNameCur;
    public TextMeshProUGUI infoDescriptionCur;

    private List<InventorySlot> slots = new List<InventorySlot>();

    private void Awake()
    {
        Instance = this;
    }

    private static bool isInit = false;
    private void OnEnable()
    {
        InitSlots();
        LoadFromPlayerInventory();
    }

    private static readonly int SIZE_INVEN = 48;
    private void InitSlots()
    {
        if (isInit)
        {
            return;
        }
        isInit = true;
        slots.Clear();
        for (int i = 0; i < SIZE_INVEN; i++)
        {
            InventorySlot slot = Instantiate(slotPrefab, slotParent);
            slots.Add(slot);
        }
        ClearInfoCur();
    }


    public void LoadFromPlayerInventory()
    {
        Debug.Log("[INV] LoadFromPlayerInventory");

        if (ClientReceiveMessageHandler.Player == null ||
            ClientReceiveMessageHandler.Player.InventoryItems == null)
        {
            Debug.LogWarning("[INV] Inventory data not ready");
            return;
        }

        foreach (var slot in slots)
            slot.Clear();

        ClearInfoCur();

        BaseItem[] items = ClientReceiveMessageHandler.Player.InventoryItems;

        for (int i = 0; i < items.Length && i < slots.Count; i++)
        {
            BaseItem item = items[i];
            if (item == null) continue;

            SetItemToSlot(i, item);
        }
    }


    private void SetItemToSlot(int slotIndex, BaseItem item)
    {
        ItemType type = item.GetItemType();

        Sprite sprite = GetSprite(item.GetIcon(), type);
        if (sprite == null)
        {
            Debug.LogWarning($"[INV] Missing sprite iconId={item.GetIcon()}");
            return;
        }
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

    public void ShowInfo(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
        {
            ClearInfoCur();
            return;
        }

        BaseItem item = slot.Item;
        Sprite sprite = slot.GetSprite();

        infoIconCur.enabled = sprite != null;
        infoIconCur.sprite = sprite;

        infoNameCur.text = item.GetName();


        infoDescriptionCur.text = $"Cấp yêu cầu: {item.GetLevelRequired()}\n{item.GetDescription()}";
    }



    public void ClearInfoCur()
    {
        infoIconCur.enabled = false;
        infoIconCur.sprite = null;
        infoNameCur.text = "";
        infoDescriptionCur.text = "";
    }
}
