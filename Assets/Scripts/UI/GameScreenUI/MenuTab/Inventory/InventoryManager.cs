using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine.Events;
using System;
using Assets.Scripts.Networking;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Slots")]
    public Transform slotParent;
    public InventorySlot slotPrefab;

    [Header("Info Panel")]
    public GameObject infoPanelRoot;
    public Image infoIconCur;
    public TextMeshProUGUI infoNameCur;
    public TextMeshProUGUI infoDescriptionCur;
    public TextMeshProUGUI useButtonText;

    private readonly List<InventorySlot> slots = new List<InventorySlot>();
    private InventorySlot selectedSlot;

    private const int SIZE_INVEN = 48;
    private Coroutine loadRoutine;

    [SerializeField] private Button btnUse;
    [SerializeField] private Button btnVutBo;


    private void Awake()
    {
        Instance = this;
        btnUse?.onClick.AddListener(UseItem);
        btnVutBo?.onClick.AddListener(VutBoItem);
    }

    private void VutBoItem()
    {
        UIManager.Instance.ShowOK("Chưa có tính năng này");
    }

    private void UseItem()
    {
        if(selectedSlot == null)
        {
            return;
        }
        RequestManager.UseItem(selectedSlot.Item.Id, selectedSlot.Item.GetItemType());
    }

    public static bool isLoadImediatetly = true;

    private void OnEnable()
    {
        isLoadImediatetly = true;

        if (infoPanelRoot != null)
            infoPanelRoot.SetActive(false);

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

        if (infoPanelRoot != null)
            infoPanelRoot.SetActive(false);

        ClearInfoCur();
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

    private void Update()
    {
        if (isLoadImediatetly)
        {
            LoadFromPlayerInventory();
        }
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
        isLoadImediatetly = false;
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

    private void ShowInfo(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
        {
            ClearInfoCur();
            return;
        }

        if (infoPanelRoot != null)
            infoPanelRoot.SetActive(true);

        BaseItem item = slot.Item;

        infoIconCur.enabled = true;
        infoIconCur.sprite = slot.GetSprite();
        infoNameCur.text = item.GetName();

        if(item.GetItemType() == ItemType.Equipment)
        {
            var gender = TemplateManager.ItemEquipmentTemplates[item.TemplateId].Gender;
            var classType = TemplateManager.ItemEquipmentTemplates[item.TemplateId].ClassType;
            var className = "Chiến binh";
            if(classType == ClassType.SAT_THU)
            {
                className = "Sát thủ";
            }
            else if(classType == ClassType.PHAP_SU)
            {
                className = "Pháp sư";
            }
            else if(classType == ClassType.XA_THU)
            {
                className = "Xạ thủ";
            }

            if (gender != Gender.Unisex)
            {
                infoDescriptionCur.text = $"Giới tính: {gender.ToString()}\n";
            }
            if(classType != ClassType.NONE)
            {
                infoDescriptionCur.text = $"Phái: {className}\n";
            }
            infoDescriptionCur.text+=
          $"Cấp yêu cầu: {item.GetLevelRequired()}\n{item.GetDescription()}";
        }
        else
        {
            infoDescriptionCur.text =
          $"Cấp yêu cầu: {item.GetLevelRequired()}\n{item.GetDescription()}";
        }
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
        useButtonText.text = "Sử dụng";
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
        //useButtonText.text = "";
    }
}
