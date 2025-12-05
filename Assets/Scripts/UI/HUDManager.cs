using UnityEngine;

public class HUDManager : MonoBehaviour
{
    [Header("HP / MP")]
    public RectTransform hpBar;
    public RectTransform mpBar;

    private float maxHPWidth;
    private float maxMPWidth;

    [Header("Skill Slots")]
    public SkillSlot[] skillSlots = new SkillSlot[8];

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
    }

    void UpdateHPMP()
    {
        Player player = GameHandler.Player;
        if (player == null) return;

        float hpRatio = player.CurrentHp / player.MaxHp;
        hpBar.sizeDelta = new Vector2(maxHPWidth * hpRatio, hpBar.sizeDelta.y);

        float mpRatio = player.CurrentMp / player.MaxMp;
        mpBar.sizeDelta = new Vector2(maxMPWidth * mpRatio, mpBar.sizeDelta.y);
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

    // Cho game gán skill
    public void SetSkill(int slot, int skillId, Sprite icon)
    {
        if (slot < 1 || slot > 8)
        {
            Debug.LogError("Slot phải từ 1–8");
            return;
        }

        skillSlots[slot - 1].AssignSkill(skillId, icon);
    }
}
