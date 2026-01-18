using UnityEngine;
using TMPro;

public class CharacterTabUIManager : MonoBehaviour
{
    [Header("Stats")]
    public TextMeshProUGUI txtPoint;
    public TextMeshProUGUI txtHP;
    public TextMeshProUGUI txtMP;
    public TextMeshProUGUI txtATK;
    public TextMeshProUGUI txtDEF;

    [Header("Equipment Slots")]
    public CharacterEquipSlot slotWeapon;
    public CharacterEquipSlot slotArmor;
    public CharacterEquipSlot slotPants;

    [Header("Player Preview")]
    public PlayerPreview preview;

    private Player player;
    private void OnEnable()
    {
        player = ClientReceiveMessageHandler.Player;
        if (player == null)
        {
            Debug.LogWarning("[CHAR TAB] Player NULL");
            return;
        }

        LoadEquipments();
        if (preview == null)
        {
            Debug.LogError("[CHAR TAB] PlayerPreview component chưa được gán!");
            return;
        }

        preview.Show(player);
    }

    private void Update()
    {
        Player p = ClientReceiveMessageHandler.Player;
        if (p != null)
        {
            txtPoint.text = "Điểm tiềm năng: " + p.PotentialPoint.ToString();
            txtHP.text = "HP: " + p.StatHp.ToString();
            txtMP.text = "MP: " + p.StatMp.ToString();
            txtATK.text = "ATK: " + p.StatAtk.ToString();
            txtDEF.text = "DEF: " + p.StatDef.ToString();
        }
    }

    private void LoadEquipments()
    {
        slotWeapon.Clear();
        slotArmor.Clear();
        slotPants.Clear();

        ItemEquipment weapon = player.GetVuKhi();
        ItemEquipment armor = player.GetAo();
        ItemEquipment pants = player.GetQuan();

        slotWeapon.SetItem(weapon);
        slotArmor.SetItem(armor);
        slotPants.SetItem(pants);
    }
    private void OnDisable()
    {
        preview.Clear();
    }

}
