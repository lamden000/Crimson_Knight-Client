using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Tạo 40 slot
        for (int i = 0; i < 40; i++)
        {
            InventorySlot newSlot = Instantiate(slotPrefab, slotParent);
            slots.Add(newSlot);

            // Gán test item mỗi 3 slot
            if (i % 3 == 0)
            {
                newSlot.SetItem(new ItemData(
                    i,          
                    i,          
                    "Item " + i,
                    "Mô tả item " + i
                ));
            }
        }

        ClearInfo();
    }

    public void ShowInfo(ItemData item)
    {
        if (item == null)
        {
            ClearInfo();
            return;
        }

        infoIcon.enabled = true;
        infoIcon.sprite = LoadIconById(item.spriteId);

        infoName.text = item.name;
        infoDescription.text = item.description;
    }

    public void ClearInfo()
    {
        infoIcon.enabled = false;
        infoName.text = "";
        infoDescription.text = "";
    }

    public Sprite LoadIconById(int id)
    {
        Sprite s = Resources.Load<Sprite>($"UI/Items/{id}");

        if (s == null)
            Debug.LogWarning($"Không tìm thấy sprite ID {id} trong Resources/UI/Items/");

        return s;
    }
}
