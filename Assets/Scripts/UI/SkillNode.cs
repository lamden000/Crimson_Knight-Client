using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillNode : MonoBehaviour
{
    public Image icon;
    public TMP_Text levelText;

    public SkillData data;

    public void Load(SkillData skill)
    {
        data = skill;

        icon.sprite = Resources.Load<Sprite>($"UI/Skills/{skill.spriteId}");
        levelText.text = "Lv " + skill.level;
    }
}
