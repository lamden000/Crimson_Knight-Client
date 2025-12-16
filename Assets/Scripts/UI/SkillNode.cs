using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillNode : MonoBehaviour
{
    public Button button;
    public Image icon;
    public TMP_Text levelText;

    public SkillData data;
    public SkillInfoPanel infoPanel;

    public void Load(SkillData skill, SkillInfoPanel panel)
    {
        data = skill;
        infoPanel = panel;

        icon.sprite = Resources.Load<Sprite>($"UI/Skills/{skill.spriteId}");
        levelText.text = "Lv " + skill.level;

        // GẮN CLICK NGAY TẠI ĐÂY
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);

        Debug.Log($"SkillNode LOAD: {skill.id} - {skill.name}");
    }

    void OnClick()
    {
        if (data == null)
        {
            Debug.LogError("SkillNode click nhưng data = NULL");
            return;
        }

        Debug.Log($"CLICK SKILL NODE ID = {data.id}");

        if (infoPanel != null)
            infoPanel.ShowSkill(data);
        else
            Debug.LogError("SkillInfoPanel chưa được gán");
    }
}

