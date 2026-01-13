using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;
using Assets.Scripts;

public class SkillUIManager : MonoBehaviour
{
    public static SkillUIManager Instance;

    [Header("Skill List")]
    public Transform skillListParent;
    public SkillNode skillNodePrefab;

    [Header("Info Panel")]
    public Image infoIcon;
    public TextMeshProUGUI infoName;
    public TextMeshProUGUI infoDesc;
    public TextMeshProUGUI infoDetail;

    private bool loaded = false;
    private Skill currentSkill;

    private void Awake()
    {
        Instance = this;
    }

    public void EnsureLoaded()
    {
        if (loaded) return;

        Debug.Log("[SKILL] EnsureLoaded");
        LoadSkillList();
        ClearInfo();

        loaded = true;
    }

    public void Reload()
    {
        Debug.Log("[SKILL] Reload");
        loaded = false;

        foreach (Transform t in skillListParent)
            Destroy(t.gameObject);

        EnsureLoaded();
    }
    private void LoadSkillList()
    {
        Player player = ClientReceiveMessageHandler.Player;
        if (player == null)
        {
            Debug.LogWarning("[SKILL] Player NULL");
            return;
        }

        foreach (Skill skill in player.Skills)
        {
            SkillNode node = Instantiate(skillNodePrefab, skillListParent);
            node.Init(skill);
        }
    }

    public void ShowSkillInfo(Skill skill)
    {
        currentSkill = skill;

        SkillTemplate template = skill.GetTemplate();
        SkillTemplate.Variant variant = template.Variants[skill.VariantId];

        // ICON
        if (ResourceManager.SkillIcons.TryGetValue(template.IconId, out Sprite sp))
        {
            infoIcon.sprite = sp;
            infoIcon.enabled = true;
            infoIcon.color = Color.white;
        }
        else
        {
            infoIcon.enabled = false;
        }

        infoName.text = template.Name;
        infoDesc.text = template.Description;

        StringBuilder sb = new StringBuilder();

        sb.AppendLine($"MP tiêu hao: {variant.MpLost}");
        sb.AppendLine($"Hồi chiêu: {variant.Cooldown / 1000f:0.0} giây");
        sb.AppendLine($"Tầm đánh: {variant.Range}");
        sb.AppendLine($"Số mục tiêu: {variant.TargetCount}");

        if (variant.Stats != null)
        {
            foreach (var stat in variant.Stats)
            {
                sb.AppendLine(GetStatText(stat.Key, stat.Value.Value));
            }
        }

        infoDetail.text = sb.ToString();

        Debug.Log($"[SKILL][INFO] {template.Name}");
    }
    public void ClearInfo()
    {
        currentSkill = null;
        infoIcon.enabled = false;
        infoName.text = "";
        infoDesc.text = "";
        infoDetail.text = "";
    }

    private string GetStatText(StatId id, int value)
    {
        switch (id)
        {
            case StatId.HP: return $"HP +{value}";
            case StatId.MP: return $"MP +{value}";
            case StatId.ATK: return $"Tấn công +{value}";
            case StatId.DEF: return $"Phòng thủ +{value}";

            case StatId.PERCENT_HP: return $"HP +{value}%";
            case StatId.PERCENT_MP: return $"MP +{value}%";
            case StatId.PERCENT_ATK: return $"Tấn công +{value}%";
            case StatId.PERCENT_DEF: return $"Phòng thủ +{value}%";
        }
        return "";
    }
}
