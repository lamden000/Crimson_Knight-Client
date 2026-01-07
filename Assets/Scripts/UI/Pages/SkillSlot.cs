using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("Skill Data")]
    public int slotIndex;         
    public int skillId = -1;      
    public Sprite skillIcon;

    [Header("UI")]
    public Image icon;
    public Image cooldownOverlay;

    [Header("Cooldown")]
    public float cooldownTime = 3f;
    private float cooldownRemaining = 0;
    public void Init(int index)
    {
        slotIndex = index;

        if (skillId < 0)
        {
            icon.sprite = null;
            cooldownOverlay.fillAmount = 0;
            return;
        }

        icon.sprite = skillIcon;
        cooldownOverlay.fillAmount = 0;
    }

    // Gán skill
    public void AssignSkill(int newSkillId, Sprite newIcon)
    {
        skillId = newSkillId;
        skillIcon = newIcon;
        icon.sprite = newIcon;

        Debug.Log($"[HUD] Đã gán skill {skillId} vào ô {slotIndex}");
    }

    void Update()
    {
        if (cooldownRemaining > 0)
        {
            cooldownRemaining -= Time.deltaTime;
            cooldownOverlay.fillAmount = cooldownRemaining / cooldownTime;
        }
    }

    public void TryUseSkill()
    {
        if (skillId < 0)
        {
            Debug.Log($"Ô skill {slotIndex} chưa gán skill!");
            return;
        }

        if (cooldownRemaining > 0)
        {
            Debug.Log($"Skill {skillId} (slot {slotIndex}) đang hồi ({cooldownRemaining:F1}s)");
            return;
        }

        Debug.Log($"DÙNG skill id {skillId} từ ô {slotIndex}");

        cooldownRemaining = cooldownTime;
        cooldownOverlay.fillAmount = 1;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"🖱 Click vào ô {slotIndex}");
        TryUseSkill();
    }
}
