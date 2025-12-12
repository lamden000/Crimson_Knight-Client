using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillInfoPanel : MonoBehaviour
{
    public Image icon;
    public TMP_Text nameText;
    public TMP_Text descText;

    public Button addPointBtn;
    public Button assignBtn;

    SkillData current;

    public void ShowSkill(SkillData data)
    {
        current = data;

        icon.sprite = Resources.Load<Sprite>($"UI/Skills/{data.spriteId}");
        nameText.text = data.name + " (Lv " + data.level + ")";
        descText.text = data.description;

        gameObject.SetActive(true);

        addPointBtn.onClick.RemoveAllListeners();
        addPointBtn.onClick.AddListener(LevelUp);

        assignBtn.onClick.RemoveAllListeners();
        assignBtn.onClick.AddListener(AssignSkill);
    }

    void LevelUp()
    {
        if (current.level < current.maxLevel)
        {
            current.level++;
            ShowSkill(current); // refresh UI
        }
    }

    void AssignSkill()
    {
        Debug.Log("Gán skill id: " + current.id);
    }
}
