using Assets.Scripts;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

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
        var template = skill.GetTemplate();
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
        bool isLearned = skill.IsLearned;
        if (!isLearned)
        {
            sb.AppendLine($"Chưa học kỹ năng này");
            sb.AppendLine($"Level yêu cầu: {skill.GetTemplate().LevelPlayerRequire}");
        }
        else
        {
            int lvCur = 1 + skill.VariantId;
            sb.Append($"Cấp độ skill hiện tại {lvCur}");
            if(skill.VariantId == template.Variants.Count - 1)
            {
                sb.AppendLine(" (Đã tối đa)");
            }
            else
            {
                sb.AppendLine();
            }
            sb.AppendLine($"MP tiêu hao: {skill.GetMpLost()}");
            sb.AppendLine($"Hồi chiêu: {skill.GetCooldown() / 1000f:0.0} giây");
            sb.AppendLine($"Tầm đánh: {skill.GetRange()}");
            sb.AppendLine($"Số mục tiêu: {skill.GetTargetCount()}");
            var stats = skill.GetStats();
            foreach (var stat in stats.Values)
            {
                StatDefinition statDefinition = TemplateManager.StatDefinitions[stat.Id];
                string content = statDefinition.Name + ": ";
                if (statDefinition.IsPercent)
                {
                    content += MathUtil.ToPercentString(stat.Value);
                }
                else
                {
                    content += stat.Value;
                }
                sb.AppendLine(content);
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
