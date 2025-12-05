using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SkillSlot : MonoBehaviour
{
    public int skillIndex;                
    public string skillName = "Skill";
    public float cooldownTime = 3f;

    public Image icon;
    public Image cooldownOverlay;

    float cooldownRemaining = 0;

    public void Init()
    {
        Sprite s = Resources.Load<Sprite>($"UI/Skills/skill{skillIndex}");
        if (s != null) icon.sprite = s;

        cooldownOverlay.fillAmount = 0;
    }

    void Update()
    {
        if (cooldownRemaining > 0)
        {
            cooldownRemaining -= Time.deltaTime;
            cooldownOverlay.fillAmount = cooldownRemaining / cooldownTime;
        }
        KeyCode key = KeyCode.Alpha1 + (skillIndex - 1);
        if (Input.GetKeyDown(key))
        {
            TryUseSkill();
        }
    }

    public void TryUseSkill()
    {
        if (cooldownRemaining > 0)
        {
            Debug.Log($"{skillName}{skillIndex} is on cooldown ({cooldownRemaining:F1}s)");
            return;
        }

        Debug.Log($"Using skill: {skillName}{skillIndex}");

        cooldownRemaining = cooldownTime;
        cooldownOverlay.fillAmount = 1;
    }
}
