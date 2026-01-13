using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Assets.Scripts;

public class SkillNode : MonoBehaviour, IPointerClickHandler
{
    public Image icon;
    public TextMeshProUGUI levelText;

    private Skill skill;

    public void Init(Skill skill)
    {
        this.skill = skill;

        SkillTemplate template = skill.GetTemplate();

        if (ResourceManager.SkillIcons.TryGetValue(template.IconId, out Sprite sp))
        {
            icon.sprite = sp;
            icon.enabled = true;
            icon.color = Color.white;
        }
        else
        {
            icon.enabled = false;
        }

        levelText.text = $"Lv {skill.VariantId + 1}";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SkillUIManager.Instance.ShowSkillInfo(skill);
        Debug.Log($"[SKILL][CLICK] {skill.GetTemplate().Name}");
    }
}
