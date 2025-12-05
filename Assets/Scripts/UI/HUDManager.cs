using UnityEngine;

public class HUDManager : MonoBehaviour
{

    public RectTransform hpBar;
    public RectTransform mpBar;

    private float maxHPWidth;
    private float maxMPWidth;

    [Header("Skills")]
    public SkillSlot[] skillSlots = new SkillSlot[8];

    void Start()
    {
        maxHPWidth = hpBar.sizeDelta.x;
        maxMPWidth = mpBar.sizeDelta.x;

        for (int i = 0; i < skillSlots.Length; i++)
        {
            skillSlots[i].skillIndex = i + 1;
            skillSlots[i].Init();
        }
    }

    void Update()
    {
        Player player = GameHandler.Player;
        if (player == null || player.MaxHp <= 0 || player.MaxMp <= 0) return;
        // Cập nhật HP
        float hpRatio = player.CurrentHp / player.MaxHp;
        hpBar.sizeDelta = new Vector2(maxHPWidth * hpRatio, hpBar.sizeDelta.y);

        // Cập nhật MP
        float mpRatio = player.CurrentMp / player.MaxMp;
        mpBar.sizeDelta = new Vector2(maxMPWidth * mpRatio, mpBar.sizeDelta.y);

        UpdateSkillInputs();
    }

    void UpdateSkillInputs()
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
}
