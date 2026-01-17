using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Assets.Scripts;
using System;

public class ShopTabManager : BaseUIManager
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
        LoadItems();
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

    public void LoadItems()
    {
        foreach (var slot in slots)
            slot.Clear();

        ClearInfoCur();

        List<Tuple<BaseItem, int>> items = new List<Tuple<BaseItem, int>>();

        foreach (var itemshop in TemplateManager.ItemShops)
        {
            BaseItem item = null;
            if (itemshop.ItemType == ItemType.Equipment)
            {
                item = new ItemEquipment(itemshop.IdItem.ToString(), itemshop.IdItem);
            }
            else if (itemshop.ItemType == ItemType.Consumable)
            {
                item = new ItemConsumable(itemshop.IdItem, 1);
            }
            else if (itemshop.ItemType == ItemType.Material)
            {
                item = new ItemMaterial(itemshop.IdItem, 1);
            }
            if(item != null)
            {
                items.Add(new Tuple<BaseItem, int>(item, itemshop.Price));
            }
        }

        for (int i = 0; i < items.Count && i < slots.Count; i++)
        {
            if (items[i] != null)
                SetItemToSlot(i, items[i].Item1, items[i].Item2);
        }
    }

    private void SetItemToSlot(int slotIndex, BaseItem item, int price)
    {
        Sprite sprite = GetSprite(item.GetIcon(), item.GetItemType());
        if (sprite == null) return;

        slots[slotIndex].SetItem(item, sprite, price);
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
            $"Giá bán: {slot.Price} vàng\nCấp yêu cầu: {item.GetLevelRequired()}\n{item.GetDescription()}";
        if (slot.Item.GetItemType() == ItemType.Equipment)
        {
            var stats = TemplateManager.ItemEquipmentTemplates[item.TemplateId].Stats;
            foreach (var stat in stats.Values)
            {
                StatDefinition statDefinition = TemplateManager.StatDefinitions[stat.Id];
                string content = statDefinition.Name + ": ";
                if (statDefinition.IsPercent)
                {
                    content += MathUtil.ToPercentString(stat.Value);
                }
                else
                {
                    content += stat.Value;
                }
                infoDescriptionCur.text += "\n" + content;
            }
        }
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
                useButtonText.text = "Mua";
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
