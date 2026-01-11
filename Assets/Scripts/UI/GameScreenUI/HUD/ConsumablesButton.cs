using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts;

public class ConsumableButton : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button useButton;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private TextMeshProUGUI quantityText;

    [Header("Item Settings")]
    [SerializeField] private string itemId; 

    [Header("Cooldown Settings")]
    [SerializeField] private float cooldownTime = 5f;

    private bool isCooldown = false;
    private float cooldownTimer = 0f;
    private void Start() { LoadQuantity(); }
    private void Awake()
    {
        if (useButton != null)
            useButton.onClick.AddListener(OnUseClicked);

        if (cooldownOverlay != null)
        {
            cooldownOverlay.enabled = false;
            cooldownOverlay.fillAmount = 0f;
        }

        LoadQuantity();
    }

    private bool hasLoadedQuantity = false;
    private void Update()
    {
        if (!hasLoadedQuantity)
        {
            int quantity = GetQuantityByItemId(itemId);
            if (quantity > 0)
            {
                LoadQuantity();
                hasLoadedQuantity = true;
            }
        }

        if (isCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownOverlay != null)
                cooldownOverlay.fillAmount = cooldownTimer / cooldownTime;

            if (cooldownTimer <= 0f)
            {
                isCooldown = false;
                if (cooldownOverlay != null)
                {
                    cooldownOverlay.enabled = false;
                    cooldownOverlay.fillAmount = 0f;
                }
            }
        }
    }

    private void OnUseClicked()
    {
        if (isCooldown) return;

        int quantity = GetQuantityByItemId(itemId);
        if (quantity <= 0)
        {
            Debug.LogWarning("[ConsumableButton] Không còn consumable để dùng!");
            return;
        }

        isCooldown = true;
        cooldownTimer = cooldownTime;

        if (cooldownOverlay != null)
        {
            cooldownOverlay.enabled = true;
            cooldownOverlay.fillAmount = 1f;
        }

        Debug.Log($"[ConsumableButton] Item {itemId} used, quantity left={quantity - 1}, cooldown started.");

        LoadQuantity();
    }

    private int GetQuantityByItemId(string id)
    {
        if (ClientReceiveMessageHandler.Player == null ||
            ClientReceiveMessageHandler.Player.InventoryItems == null)
            return 0;

        foreach (BaseItem item in ClientReceiveMessageHandler.Player.InventoryItems)
        {
            if (item == null) continue;
            if (item.Id == id && item.GetItemType() == ItemType.Consumable)
            {
                return ((ItemConsumable)item).Quantity;
            }
        }
        return 0;
    }

    public void LoadQuantity()
    {
        if (ClientReceiveMessageHandler.Player == null ||
            ClientReceiveMessageHandler.Player.InventoryItems == null)
        {
            Debug.Log("[ConsumableButton] Inventory chưa sẵn sàng");
            return;
        }

        int quantity = GetQuantityByItemId(itemId);
        if (quantityText != null)
            quantityText.text = quantity > 0 ? quantity.ToString() : "";
    }
    
}
