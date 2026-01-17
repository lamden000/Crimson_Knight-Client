using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
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
    public TextMeshProUGUI useButtonText;

    private readonly List<InventorySlot> slots = new List<InventorySlot>();
    private InventorySlot selectedSlot;

    private const int SIZE_INVEN = 48;
    private Coroutine loadRoutine;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        Time.timeScale = 1f;

        if (slots.Count == 0)
            InitSlots();

        if (loadRoutine != null)
            StopCoroutine(loadRoutine);

        loadRoutine = StartCoroutine(WaitAndLoad());
    }

    private void OnDisable()
    {
        if (loadRoutine != null)
        {
            StopCoroutine(loadRoutine);
            loadRoutine = null;
        }
    }

    private IEnumerator WaitAndLoad()
    {
        yield return new WaitUntil(() =>
            ClientReceiveMessageHandler.Player != null &&
            ClientReceiveMessageHandler.Player.InventoryItems != null &&
            ResourceManager.ItemEquipmentIconSprites.Count > 0
        );

        LoadFromPlayerInventory();
    }

    private void InitSlots()
    {
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
        foreach (var slot in slots)
            slot.Clear();

        ClearInfoCur();

        BaseItem[] items = ClientReceiveMessageHandler.Player.InventoryItems;

        for (int i = 0; i < items.Length && i < slots.Count; i++)
        {
            if (items[i] == null) continue;
            SetItemToSlot(i, items[i]);
        }
    }

    private void SetItemToSlot(int index, BaseItem item)
    {
        Sprite sprite = GetSprite(item.GetIcon(), item.GetItemType());
        if (sprite == null) return;

        slots[index].SetItem(item, sprite);
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

    public void SelectSlot(InventorySlot slot)
    {
        if (selectedSlot != null)
            selectedSlot.SetSelected(false);

        selectedSlot = slot;
        selectedSlot.SetSelected(true);

        ShowInfo(slot);
    }

    public void ShowInfo(InventorySlot slot)
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
                useButtonText.text = "Trang bị";
                break;
            case ItemType.Consumable:
            case ItemType.Material:
                useButtonText.text = "Sử dụng";
                break;
            default:
                useButtonText.text = "Dùng";
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
        useButtonText.text = "";
    }
}
