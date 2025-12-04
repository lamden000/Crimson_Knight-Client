using UnityEngine;

public class StatusBars : MonoBehaviour
{
    public GPlayer player;

    public RectTransform hpBar;
    public RectTransform mpBar;

    private float maxHPWidth;
    private float maxMPWidth;

    void Start()
    {
        maxHPWidth = hpBar.sizeDelta.x;
        maxMPWidth = mpBar.sizeDelta.x;
    }

    void Update()
    {
        // Cập nhật HP
        float hpRatio = player.currentHP / player.maxHP;
        hpBar.sizeDelta = new Vector2(maxHPWidth * hpRatio, hpBar.sizeDelta.y);

        // Cập nhật MP
        float mpRatio = player.currentMP / player.maxMP;
        mpBar.sizeDelta = new Vector2(maxMPWidth * mpRatio, mpBar.sizeDelta.y);
    }
}
public class GPlayer : MonoBehaviour
{
    public float maxHP = 100;
    public float currentHP = 100;

    public float maxMP = 50;
    public float currentMP = 50;

    public void TakeDamage(float dmg)
    {
        currentHP -= dmg;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
    }

    public void UseMP(float amount)
    {
        currentMP -= amount;
        currentMP = Mathf.Clamp(currentMP, 0, maxMP);
    }
}