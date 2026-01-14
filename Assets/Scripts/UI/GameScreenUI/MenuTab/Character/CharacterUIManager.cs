using UnityEngine;
using TMPro;

public class CharacterTabUIManager : MonoBehaviour
{
    [Header("Stats")]
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

    //private void LoadStats()
    //{
    //    txtHP.text = $"HP: {player.GetStat(StatId.HP)}";
    //    txtMP.text = $"MP: {player.GetStat(StatId.MP)}";
    //    txtATK.text = $"Tấn công: {player.GetStat(StatId.ATK)}";
    //    txtDEF.text = $"Phòng thủ: {player.GetStat(StatId.DEF)}";
    //}


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
