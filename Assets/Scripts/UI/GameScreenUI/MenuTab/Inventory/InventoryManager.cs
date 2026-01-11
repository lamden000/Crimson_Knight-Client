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
    public Image infoIcon;
    public TextMeshProUGUI infoName;
    public TextMeshProUGUI infoDescription;

    private List<InventorySlot> slots = new List<InventorySlot>();
    private bool initialized = false;

    private static Dictionary<int, ItemType> templateTypeMap;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        Debug.Log("[INV] Inventory Open");

        if (!initialized)
        {
            InitSlots();
            BuildTemplateTypeMap();
            initialized = true;
        }

        LoadFromPlayerInventory();
    }


    private void InitSlots()
    {
        slots.Clear();

        for (int i = 0; i < 48; i++)
        {
            InventorySlot slot = Instantiate(slotPrefab, slotParent);
            slot.Init(i);
            slot.Clear();
            slots.Add(slot);
        }

        ClearInfo();
    }

    private void BuildTemplateTypeMap()
    {
        templateTypeMap = new Dictionary<int, ItemType>();

        for (int i = 0; i < TemplateManager.ItemEquipmentTemplates.Count; i++)
            templateTypeMap[i] = ItemType.Equipment;

        for (int i = 0; i < TemplateManager.ItemConsumableTemplates.Count; i++)
            templateTypeMap[i] = ItemType.Consumable;

        for (int i = 0; i < TemplateManager.ItemMaterialTemplates.Count; i++)
            templateTypeMap[i] = ItemType.Material;

        Debug.Log($"[INV] TemplateTypeMap built: {templateTypeMap.Count}");
    }

    private ItemType GetItemType(int templateId)
    {
        if (templateTypeMap.TryGetValue(templateId, out var type))
            return type;

        Debug.LogError($"[INV] Unknown templateId={templateId}");
        return ItemType.Material;
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

        ClearInfo();

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
        int templateId = item.TemplateId;
        ItemType type = item.GetItemType();

        int iconId = -1;
        string name = "";
        string desc = "";

        switch (type)
        {
            case ItemType.Equipment:
                iconId = TemplateManager.ItemEquipmentTemplates[templateId].IconId;
                name = TemplateManager.ItemEquipmentTemplates[templateId].Name;
                desc = TemplateManager.ItemEquipmentTemplates[templateId].Description;
                break;

            case ItemType.Consumable:
                iconId = TemplateManager.ItemConsumableTemplates[templateId].IconId;
                name = TemplateManager.ItemConsumableTemplates[templateId].Name;
                desc = TemplateManager.ItemConsumableTemplates[templateId].Description;
                break;

            case ItemType.Material:
                iconId = TemplateManager.ItemMaterialTemplates[templateId].IconId;
                name = TemplateManager.ItemMaterialTemplates[templateId].Name;
                desc = TemplateManager.ItemMaterialTemplates[templateId].Description;
                break;
        }

        // ...
        Sprite sprite = GetSprite(iconId, type);
        if (sprite == null)
        {
            Debug.LogWarning($"[INV] Missing sprite iconId={iconId}");
            return;
        }

        ItemData data = new ItemData(templateId, iconId, name, desc);
        slots[slotIndex].SetItem(data, sprite);

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
        if (slot == null || slot.GetItemData() == null)
        {
            ClearInfo();
            return;
        }

        ItemData item = slot.GetItemData();
        Sprite sprite = slot.GetSprite();

        infoIcon.enabled = sprite != null;
        infoIcon.sprite = sprite;

        infoName.text = item.name;

        int levelRequire = 0;
        ItemType type = GetItemType(item.itemId);

        switch (type)
        {
            case ItemType.Equipment:
                levelRequire = TemplateManager.ItemEquipmentTemplates[item.itemId].LevelRequire;
                break;
            case ItemType.Consumable:
                levelRequire = TemplateManager.ItemConsumableTemplates[item.itemId].LevelRequire;
                break;
            case ItemType.Material:
                levelRequire = TemplateManager.ItemMaterialTemplates[item.itemId].LevelRequire;
                break;
        }

        infoDescription.text = $"Cấp yêu cầu: {levelRequire}\n{item.description}";

        Debug.Log($"[INV][INFO] template={item.itemId} iconId={item.spriteId} level={levelRequire}");
    }



    public void ClearInfo()
    {
        infoIcon.enabled = false;
        infoIcon.sprite = null;
        infoName.text = "";
        infoDescription.text = "";
    }
}
