using UnityEngine;
using UnityEngine.UI;

public class ConsumableButton : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button useButton; 
    [SerializeField] private Image cooldownOverlay; 

    [Header("Cooldown Settings")]
    [SerializeField] private float cooldownTime = 5f; 

    private bool isCooldown = false;
    private float cooldownTimer = 0f;

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
        isCooldown = true;
        cooldownTimer = cooldownTime;

        if (cooldownOverlay != null)
        {
            cooldownOverlay.enabled = true;
            cooldownOverlay.fillAmount = 1f;
        }

        Debug.Log("[ConsumableButton] Item used, cooldown started.");
    }
}
