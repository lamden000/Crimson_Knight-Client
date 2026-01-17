using Assets.Scripts;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUIManager : MonoBehaviour
{
    public static SkillUIManager Instance;

    [Header("Skill List")]
    public Transform skillListParent;
    public SkillNode skillNodePrefab;

    [Header("Info Panel")]
    public GameObject infoPanel; // GameObject chứa toàn bộ skill info panel
    public Image infoIcon;
    public TextMeshProUGUI infoName;
    public TextMeshProUGUI infoDesc;
    public TextMeshProUGUI infoDetail;

    [Header("Preview")]
    public Button previewButton;
    public Transform previewRoot; // Root transform cho skill preview area
    public PlayerPreview playerPreview; // Reference đến PlayerPreview để dùng chung camera

    [Header("Dummy Objects")]
    public GameObject dummyCaster; // Dummy caster có sẵn trong scene
    public GameObject dummyTarget;  // Dummy target có sẵn trong scene

    private bool loaded = false;
    private Skill currentSkill;
    private float previewCooldown = 0f;
    private const float PREVIEW_COOLDOWN_TIME = 3f;

    private void Awake()
    {
        Instance = this;
        
        if (previewButton != null)
        {
            previewButton.onClick.AddListener(OnPreviewButtonClicked);
        }

        // Ẩn skill info panel mặc định
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // Update cooldown
        if (previewCooldown > 0f)
        {
            previewCooldown -= Time.deltaTime;
            if (previewCooldown <= 0f)
            {
                previewCooldown = 0f;
                if (previewButton != null)
                {
                    previewButton.interactable = true;
                }
            }
        }
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
        
        // Chỉnh size camera cho skill preview
        if (playerPreview != null)
        {
            playerPreview.SetCameraSizeForSkill();
        }

        // Bật skill info panel
        if (infoPanel != null)
        {
            infoPanel.SetActive(true);
        }

        // Bật dummy caster và target
        if (dummyCaster != null && !dummyCaster.activeInHierarchy)
        {
            dummyCaster.SetActive(true);
            // Đảm bảo dummy caster ở đúng layer để camera nhìn thấy
            int layer = LayerMask.NameToLayer("PlayerPreview");
            SetLayerRecursively(dummyCaster, layer);
        }

        if (dummyTarget != null && !dummyTarget.activeInHierarchy)
        {
            dummyTarget.SetActive(true);
            // Đảm bảo dummy target ở đúng layer để camera nhìn thấy
            int layer = LayerMask.NameToLayer("PlayerPreview");
            SetLayerRecursively(dummyTarget, layer);
        }
        
        // Enable preview button nếu skill đã học
        if (previewButton != null)
        {
            previewButton.interactable = skill.IsLearned && previewCooldown <= 0f;
        }
        
        var template = skill.GetTemplate();
        
        // Ánh xạ icon
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

        // Ánh xạ tên
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
        
        // Ẩn skill info panel
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }
        
        // Reset camera size về character preview
        if (playerPreview != null)
        {
            playerPreview.SetCameraSizeForCharacter();
        }
        
        // Clear preview
        ClearPreview();
    }

    private void OnDisable()
    {
        // Clear preview khi tab đóng
        ClearPreview();
    }

    private void OnPreviewButtonClicked()
    {
        if (currentSkill == null)
        {
            Debug.LogWarning("[SKILL] Không có skill được chọn để preview!");
            return;
        }

        if (!currentSkill.IsLearned)
        {
            Debug.LogWarning("[SKILL] Skill chưa được học, không thể preview!");
            return;
        }

        if (previewCooldown > 0f)
        {
            Debug.LogWarning($"[SKILL] Preview đang trong cooldown: {previewCooldown:F1}s");
            return;
        }

        PreviewSkill(currentSkill);
    }

    private void PreviewSkill(Skill skill)
    {
        if (previewRoot == null)
        {
            Debug.LogError("[SKILL] Preview root chưa được gán!");
            return;
        }

        if (playerPreview == null)
        {
            Debug.LogError("[SKILL] PlayerPreview chưa được gán!");
            return;
        }

        if (dummyCaster == null)
        {
            Debug.LogError("[SKILL] Dummy caster chưa được gán!");
            return;
        }

        if (dummyTarget == null)
        {
            Debug.LogError("[SKILL] Dummy target chưa được gán!");
            return;
        }

        // Set camera size cho skill preview
        playerPreview.SetCameraSizeForSkill();

        // Enable dummy caster và target
        if (!dummyCaster.activeInHierarchy)
        {
            dummyCaster.SetActive(true);
        }

        if (!dummyTarget.activeInHierarchy)
        {
            dummyTarget.SetActive(true);
        }

        // Đảm bảo dummy objects ở đúng layer để camera nhìn thấy
        int layer = LayerMask.NameToLayer("PlayerPreview");
        SetLayerRecursively(dummyCaster, layer);
        SetLayerRecursively(dummyTarget, layer);

        // Spawn skill
        var template = skill.GetTemplate();
        if (template != null && !string.IsNullOrEmpty(template.EffectName))
        {
            SpawnManager.GI().SpawnEffectPrefab(
                template.EffectName,
                dummyCaster.transform,
                dummyTarget.transform,
                0.5f // duration
            );
        }

        // Set cooldown
        previewCooldown = PREVIEW_COOLDOWN_TIME;
        if (previewButton != null)
        {
            previewButton.interactable = false;
        }

        Debug.Log($"[SKILL] Preview skill: {skill.GetTemplate().Name}");
    }

    private void ClearPreview()
    {
        // Disable dummy caster và target (không destroy vì chúng là object có sẵn trong scene)
        if (dummyCaster != null)
        {
            dummyCaster.SetActive(false);
        }

        if (dummyTarget != null)
        {
            dummyTarget.SetActive(false);
        }

        // Reset camera size về character preview
        if (playerPreview != null)
        {
            playerPreview.SetCameraSizeForCharacter();
        }
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
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
