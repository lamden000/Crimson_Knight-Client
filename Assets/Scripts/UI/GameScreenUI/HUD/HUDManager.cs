using Assets.Scripts;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUDManager : BaseUIManager
{
    [Header("HP / MP")]
    public RectTransform hpBar;
    public RectTransform mpBar;

    private float maxHPWidth;
    private float maxMPWidth;

    [Header("HP / MP Text")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI mpText;

    [Header("Skill Slots")]
    public SkillSlot[] skillSlots = new SkillSlot[8];


    [SerializeField]
    private TextMeshProUGUI txtLevel;

    [SerializeField]
    private TextMeshProUGUI txtPtLevel;


    void Start()
    {
        maxHPWidth = hpBar.sizeDelta.x;
        maxMPWidth = mpBar.sizeDelta.x;

        for (int i = 0; i < skillSlots.Length; i++)
            skillSlots[i].Init(i + 1);

        
    }

    void Update()
    {
        UpdateHPMP();
        UpdateSkillHotkeys();
        UpdateLevelInfo();
        if (ClientReceiveMessageHandler.Player != null)
        {
            if (isLoadSkillImediatetly)
            {
                isLoadSkillImediatetly = false;
                LoadSkills();
            }
        }
    }

    public static bool isLoadSkillImediatetly = false;
    void LoadSkills()
    {
        for (int i = 0; i < ClientReceiveMessageHandler.Player.Skills.Count; i++)
        {
            Skill skill = ClientReceiveMessageHandler.Player.Skills[i];
            if (skill.IsLearned)
            {
                UIManager.Instance.gameScreenUIManager.hudManager.SetSkill(i + 1, skill);
            }
        }
    }

    private void UpdateLevelInfo()
    {
        Player p = ClientReceiveMessageHandler.Player;
        if (p == null) return;

        float percent = GetLevelPercent(p.Level, p.Exp);
        string textLv = $"Lv {p.Level}";
        string textPtLv = $"{(percent * 100f):0.##}%";
        txtLevel.text = textLv;
        txtPtLevel.text = textPtLv;
    }

    float GetLevelPercent(int level, long totalExp)
    {
        int maxLevel = TemplateManager.Levels.Count - 1;

        level = Mathf.Clamp(level, 1, maxLevel);

        if (level >= maxLevel)
            return 1f;

        int curExp = TemplateManager.Levels[level];
        int nextExp = TemplateManager.Levels[level + 1];

        totalExp = Math.Clamp(totalExp, curExp, TemplateManager.Levels[maxLevel]);

        return Mathf.Clamp01((float)(totalExp - curExp) / (float)(nextExp - curExp));
    }




    void UpdateHPMP()
    {
        Player player = ClientReceiveMessageHandler.Player;
        if (player == null) return;
        if (player.MaxHp <= 0 || player.MaxMp <= 0) return;

        float hpRatio = (float)player.CurrentHp / (float)player.MaxHp;
        hpRatio = Mathf.Clamp01(hpRatio);
        hpBar.sizeDelta = new Vector2(maxHPWidth * hpRatio, hpBar.sizeDelta.y);

        float mpRatio = (float)player.CurrentMp / (float)player.MaxMp;
        mpRatio = Mathf.Clamp01(mpRatio);
        mpBar.sizeDelta = new Vector2(maxMPWidth * mpRatio, mpBar.sizeDelta.y);

        if (hpText != null)
            hpText.text = $"{player.CurrentHp}/{player.MaxHp}";
        if (mpText != null)
            mpText.text = $"{player.CurrentMp}/{player.MaxMp}";
    }

    void UpdateSkillHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) skillSlots[0].TryUseSkill();
        if (Input.GetKeyDown(KeyCode.Alpha2)) skillSlots[1].TryUseSkill();
        if (Input.GetKeyDown(KeyCode.Alpha3)) skillSlots[2].TryUseSkill();
        if (Input.GetKeyDown(KeyCode.Alpha4)) skillSlots[3].TryUseSkill();
        if (Input.GetKeyDown(KeyCode.Alpha5)) skillSlots[4].TryUseSkill();
        if (Input.GetKeyDown(KeyCode.Alpha6)) skillSlots[5].TryUseSkill();
        if (Input.GetKeyDown(KeyCode.Alpha7)) skillSlots[6].TryUseSkill();
        if (Input.GetKeyDown(KeyCode.Alpha8)) skillSlots[7].TryUseSkill();
    }

    public void SetSkill(int slot, Skill skill)
    {
        if (slot < 1 || slot > 8)
        {
            Debug.LogError("Slot phải từ 1–8");
            return;
        }
        Sprite icon = ResourceManager.SkillIcons[skill.GetTemplate().IconId];
        skillSlots[slot - 1].AssignSkill(skill, icon);
    }
}
