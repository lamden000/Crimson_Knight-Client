using UnityEngine;

public class HUDManager : MonoBehaviour
{

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
        Player player = GameHandler.Player;
        if (player == null) return;
        // Cập nhật HP
        float hpRatio = player.CurrentHp / player.MaxHp;
        hpBar.sizeDelta = new Vector2(maxHPWidth * hpRatio, hpBar.sizeDelta.y);

        // Cập nhật MP
        float mpRatio = player.CurrentMp / player.MaxMp;
        mpBar.sizeDelta = new Vector2(maxMPWidth * mpRatio, mpBar.sizeDelta.y);
    }
}
