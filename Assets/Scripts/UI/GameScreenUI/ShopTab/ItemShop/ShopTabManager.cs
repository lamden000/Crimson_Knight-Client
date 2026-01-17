using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Assets.Scripts;
using System;

public class ShopTabManager : BaseUIManager
{
    public static ShopTabManager Instance;

    [SerializeField] private Button btnClose;

    [Header("Slots")]
    public Transform slotParent;
    public ShopItemSlot slotPrefab;

    [Header("Info Panel")]
    public Image infoIconCur;
    public TextMeshProUGUI infoNameCur;
    public TextMeshProUGUI infoDescriptionCur;
    public TextMeshProUGUI useButtonText;

    private readonly List<ShopItemSlot> slots = new List<ShopItemSlot>();
    private ShopItemSlot selectedSlot;

    private const int SIZE_INVEN = 48;

    private void Awake()
    {
        Instance = this;

        if (btnClose != null)
            btnClose.onClick.AddListener(Close);
    }

    private void OnEnable()
    {
        if (slots.Count == 0)
            InitSlots();

        LoadItems();
    }

    private void InitSlots()
    {
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
                item = new ItemEquipment(itemshop.IdItem.ToString(), itemshop.IdItem);
            else if (itemshop.ItemType == ItemType.Consumable)
                item = new ItemConsumable(itemshop.IdItem, 1);
            else if (itemshop.ItemType == ItemType.Material)
                item = new ItemMaterial(itemshop.IdItem, 1);

            if (item != null)
                items.Add(new Tuple<BaseItem, int>(item, itemshop.Price));
        }

        for (int i = 0; i < items.Count && i < slots.Count; i++)
            SetItemToSlot(i, items[i].Item1, items[i].Item2);
    }

    private void SetItemToSlot(int index, BaseItem item, int price)
    {
        Sprite sprite = GetSprite(item.GetIcon(), item.GetItemType());
        if (sprite == null) return;

        slots[index].SetItem(item, sprite, price);
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
            $"Giá bán: {slot.Price} vàng\n" +
            $"Cấp yêu cầu: {item.GetLevelRequired()}\n" +
            $"{item.GetDescription()}";

        if (item.GetItemType() == ItemType.Equipment)
        {
            var stats = TemplateManager.ItemEquipmentTemplates[item.TemplateId].Stats;
            foreach (var stat in stats.Values)
            {
                StatDefinition def = TemplateManager.StatDefinitions[stat.Id];
                string value = def.IsPercent
                    ? MathUtil.ToPercentString(stat.Value)
                    : stat.Value.ToString();

                infoDescriptionCur.text += "\n" + def.Name + ": " + value;
            }
        }

        useButtonText.text = "Mua";
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
        useButtonText.text = "";
    }

    public void Close()
    {
        gameObject.SetActive(false);
        UIManager.Instance.gameScreenUIManager.ShowHUD();
    }
}
