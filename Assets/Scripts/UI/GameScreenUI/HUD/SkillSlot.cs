using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Assets.Scripts.Utils;

public class SkillSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("Skill Data")]
    public int slotIndex;         

    [Header("UI")]
    public Image cooldownOverlay;

    Skill skill;

    [SerializeField] private Image icon;

    public void Init(int index)
    {
        slotIndex = index;
        cooldownOverlay.fillAmount = 0;
    }

    // Gán skill
    public void AssignSkill(Skill skill, Sprite newIcon)
    {
        this.skill = skill;
        icon.sprite = newIcon;
    }
    private float cooldownRemaining;
    private float cooldownTime;
    void Update()
    {
        if (skill != null && !skill.CanAttack())
        {
            long elapsed = SystemUtil.CurrentTimeMillis() - skill.StartTimeAttack;
            cooldownTime = skill.GetCooldown() / 1000f;
            cooldownRemaining = (skill.GetCooldown() - elapsed) / 1000f;

            cooldownOverlay.fillAmount = cooldownRemaining / cooldownTime;
        }
        else
        {
            cooldownOverlay.fillAmount = 0;
        }
    }

    public void TryUseSkill()
    {
        if (skill == null)
        {
            Debug.Log($"Ô skill {slotIndex} chưa gán skill!");
            return;
        }

        if (!skill.CanAttack()) 
        {
            Debug.Log("Đang hồi chiêu");
            return;
        }
        ClientReceiveMessageHandler.Player.Attack(skill.TemplateId, ClientReceiveMessageHandler.Player.objFocus);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"🖱 Click vào ô {slotIndex}");
        TryUseSkill();
    }
}
