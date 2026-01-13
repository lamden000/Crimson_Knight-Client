using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts;
using System;
using Assets.Scripts.Networking;

public class ConsumableButton : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button useButton;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private bool isBtnHp;

    [SerializeField] private Image img;

    private bool isCooldown = false;
    private float cooldownTimer = 0f;

    private float cooldown = 2f;
    private int quantity = 0;

    private string idItem = "";
    private void Awake()
    {
        if (useButton != null)
            useButton.onClick.AddListener(OnUseClicked);

        if (cooldownOverlay != null)
        {
            cooldownOverlay.enabled = false;
            cooldownOverlay.fillAmount = 0f;
        }
    }
    private void Update()
    {
        CheckInventory();
        if (isCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownOverlay != null)
                cooldownOverlay.fillAmount = cooldownTimer / cooldown;

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

    private void CheckInventory()
    {
        if (ClientReceiveMessageHandler.Player == null || ClientReceiveMessageHandler.Player.InventoryItems == null)
        {
            quantity = 0;
            return;
        }
        ItemConsumable item = null;

        if (isBtnHp)
        {
            item = (ItemConsumable)ClientReceiveMessageHandler.Player.GetItemInventoty(0, ItemType.Consumable);
            if (item == null)
            {
                item = (ItemConsumable)ClientReceiveMessageHandler.Player.GetItemInventoty(2, ItemType.Consumable);
            }
        }
        else
        {
            item = (ItemConsumable)ClientReceiveMessageHandler.Player.GetItemInventoty(1, ItemType.Consumable);
            if (item == null)
            {
                item = (ItemConsumable)ClientReceiveMessageHandler.Player.GetItemInventoty(3, ItemType.Consumable);
            }
        }

        if (item == null)
        {
            quantity = 0;
            if (isBtnHp)
            {
                img.sprite = ResourceManager.ItemConsumableIconSprites[0];
            }
            else
            {
                img.sprite = ResourceManager.ItemConsumableIconSprites[1];
            }
            idItem = "";
        }
        else
        {
            idItem = item.Id;
            quantity = item.GetQuantity();
            cooldown = (float)(TemplateManager.ItemConsumableTemplates[item.TemplateId].Cooldown)/1000;
            img.sprite = ResourceManager.ItemConsumableIconSprites[item.GetIcon()];
        }
        quantityText.text = quantity.ToString();
    }

    private void OnUseClicked()
    {
        if (quantity == 0 || idItem == "")
        {
            string name = isBtnHp ? "Hp" : "Mp";
            ClientReceiveMessageHandler.CenterNotifications.Enqueue("Đã hết bình " + name);
            return;
        }
        if (isCooldown) return;

        RequestManager.UseItem(idItem, ItemType.Consumable);
        isCooldown = true;
        cooldownTimer = cooldown + 0.3f;

        if (cooldownOverlay != null)
        {
            cooldownOverlay.enabled = true;
            cooldownOverlay.fillAmount = 1f;
        }
    }
}
