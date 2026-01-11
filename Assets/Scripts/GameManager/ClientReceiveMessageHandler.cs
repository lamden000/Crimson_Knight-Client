using Assets.Scripts.GameManager.Players;
using Assets.Scripts.Map;
using Assets.Scripts.Networking;
using Assets.Scripts.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

public class ClientReceiveMessageHandler : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        // Đợi frame đầu tiên kết thúc
        yield return null;
        Debug.Log("Start cuối cùng");
        UIManager.Init();


        MapManager.Initialize();
        MapManager.LoadMapForLoginScreen();

        //
    }



    public static Player Player;
    public static Dictionary<int, OtherPlayer> OtherPlayers = new Dictionary<int, OtherPlayer>();
    public static Dictionary<int, Monster> Monsters = new Dictionary<int, Monster>();
    public static Dictionary<int, Npc> Npcs = new Dictionary<int, Npc>();



    private void LateUpdate()
    {

    }
    public static void EnterMap(Message msg)
    {
        bool isLoadMap = msg.ReadBool();
        if (isLoadMap)
        {
            UIManager.Instance.DisableGameScreen();
            UIManager.Instance.EnableLoadScreen();
            //map
            MapManager.MapId = msg.ReadShort();
            MapManager.LoadMapById(MapManager.MapId);

            MapManager.MapName = msg.ReadString();


            //player
            short x = msg.ReadShort();
            short y = msg.ReadShort();

            Player.SetPosition(x, y);


            UIManager.Instance.DisableLoadScreen();
            UIManager.Instance.EnableGameScreen();
        }
        else
        {
            int otherPlayerId = msg.ReadInt();
            string otherPlayerName = msg.ReadString();
            short otherX = msg.ReadShort();
            short otherY = msg.ReadShort();
            ClassType classType = (ClassType)msg.ReadByte();
            Gender gender = (Gender)msg.ReadByte();
            if (!OtherPlayers.ContainsKey(otherPlayerId))
            {
                OtherPlayer player = OtherPlayer.Create(otherPlayerId, otherPlayerName, otherX, otherY, classType, gender);
                OtherPlayers.Add(otherPlayerId, player);
            }
        }
    }
    public static void PlayerMove(Message msg)
    {
        int id = msg.ReadInt();
        int x = msg.ReadShort();
        int y = msg.ReadShort();
        if (OtherPlayers.TryGetValue(id, out OtherPlayer other))
        {
            other.Moves.Enqueue(new Tuple<int, int>(x, y));
        }
        else
        {
            Debug.LogWarning("Khong tim thay other player co id la " + id);
        }
    }

    public static void OtherPlayerExitMap(int otherPlayerId)
    {
        OtherPlayers.TryGetValue(otherPlayerId, out OtherPlayer other);
        if (other != null)
        {
            other.DestroyObject();
            OtherPlayers.Remove(otherPlayerId);
        }
    }


    public static void LoadOtherPlayersInMap(Message msg)
    {
        foreach (var obj in OtherPlayers)
        {
            obj.Value.DestroyObject();
        }
        OtherPlayers.Clear();

        int size = msg.ReadShort();
        for (int i = 0; i < size; i++)
        {
            int id = msg.ReadInt();
            string name = msg.ReadString();
            ClassType classType = (ClassType)msg.ReadByte();
            short xO = msg.ReadShort();
            short yO = msg.ReadShort();
            Gender gender = (Gender)msg.ReadByte();
            if (id == Player.Id)
            {
                continue;
            }
            OtherPlayer player = OtherPlayer.Create(id, name, xO, yO,classType, gender);
            player.ClassType = classType;
            if (!OtherPlayers.TryAdd(id, player))
            {
                player.DestroyObject();
            }
        }
    }

    public static void LoadMonstersInMap(Message msg)
    {
        foreach (var obj in Monsters)
        {
            obj.Value.DestroyObject();
        }
        Monsters.Clear();
        int size = msg.ReadShort();
        for (int i = 0; i < size; i++)
        {
            int templateId = msg.ReadInt();
            int id = msg.ReadInt();
            short x = msg.ReadShort();
            short y = msg.ReadShort();
            Monsters.TryAdd(i, Monster.Create(i, x, y, templateId));
        }
    }

    public static void LoadNpcsInMap(Message msg)
    {
        foreach (var obj in Npcs)
        {
            obj.Value.DestroyObject();
        }
        Npcs.Clear();
        int size = msg.ReadShort();
        for (int i = 0; i < size; i++)
        {
            int templateId = msg.ReadInt();
            short x = msg.ReadShort();
            short y = msg.ReadShort();
            Npcs.TryAdd(i, Npc.Create(x, y, templateId));
        }
    }

    public static void ShowMenu(Message msg)
    {
        int npcId = msg.ReadInt();
        string title = msg.ReadString();
        byte size = msg.ReadByte();
        string[] arr = new string[size];
        for (int i = 0; i < size; i++)
        {
            string menuItem = msg.ReadString();
            arr[i] = menuItem;
        }
        UIManager.Instance.ShowDropdown(
        title,
        arr,
        (selectedIndex) =>
        {
            Debug.Log($"NPC ID: {npcId}, Chọn menu index: {selectedIndex}");
            RequestManager.RequestSelectMenuItem(npcId, (byte)selectedIndex);
        });
    }

    public static void PlayerAttack(Message msg)
    {
        int playerId = msg.ReadInt();
        int skillUseId = msg.ReadInt();
        int dam = msg.ReadInt();

        BaseObject attacker = null;
        if (playerId == Player.Id)
        {
            attacker = Player;
        }
        else
        {
            OtherPlayers.TryGetValue(playerId, out var value);
            attacker = value;
          
        }

        int targetSize = msg.ReadByte();
        BaseObject firstTarget = null;
        for (int i = 0; i < targetSize; i++)
        {
            bool isPlayer = msg.ReadBool();
            int targetId = msg.ReadInt();
            BaseObject target = null;
            if (isPlayer)
            {
                if (ClientReceiveMessageHandler.Player.Id == targetId)
                {
                    target = ClientReceiveMessageHandler.Player;
                    
                }
                else
                {
                    if (OtherPlayers.TryGetValue(targetId, out var otherPlayer))
                    {
                        if (otherPlayer != null && !otherPlayer.IsDie())
                        {
                            target = otherPlayer;
                        }
                    }
                }
                if (target != null && !target.IsDie() && attacker != null)
                {
                    target.AniTakeDamage(dam, attacker);
                }
            }
            else
            {
                if (Monsters.TryGetValue(targetId, out var monster))
                {
                    if (monster != null && !monster.IsDie())
                    {
                        target = monster;
                        if (attacker != null)
                        {
                            monster.AniTakeDamage(dam,attacker);
                        }
                    }
                }
            }

            if (firstTarget == null)
            {
                firstTarget = target;
            }

            if(target!=null && attacker != null && attacker.IsOtherPlayer())
            {
                OtherPlayer otherPlayer = attacker as OtherPlayer;
                SpawnManager.GI().SpawnEffectPrefab(Skill.GetSkillTemplate(skillUseId, otherPlayer.ClassType).EffectName, otherPlayer.transform, target.transform);
            }
        }

        if (attacker != null)
        {
            attacker.AniAttack(firstTarget);
        }
    }

    public static void PlayerBaseInfo(Message msg)
    {
        int playerId = msg.ReadInt();
        string name = msg.ReadString();
        short Level = msg.ReadShort();
        long Exp = msg.ReadLong();
        int CurrentHp = msg.ReadInt();
        int MaxHp = msg.ReadInt();
        int CurrentMp = msg.ReadInt();
        int MaxMp = msg.ReadInt();

        if (playerId == Player.Id)
        {
            Player.Level = Level;
            Player.Exp = Exp;
            Player.CurrentHp = CurrentHp;
            Player.MaxHp = MaxHp;
            Player.CurrentMp = CurrentMp;
            Player.MaxMp = MaxMp;
        }
        else
        {
            if (ClientReceiveMessageHandler.OtherPlayers.TryGetValue(playerId, out OtherPlayer otherPlayer))
            {
                otherPlayer.Level = Level;
                otherPlayer.CurrentHp = CurrentHp;
                otherPlayer.MaxHp = MaxHp;
            }
        }
    }

    public static void PlayerPktypeInfo(Message msg)
    {
        int playerId = msg.ReadInt();
        PkType type = (PkType)msg.ReadByte();
        if (playerId == Player.Id)
        {
            Player.ChangePkType(type);
        }
        else
        {
            if (ClientReceiveMessageHandler.OtherPlayers.TryGetValue(playerId, out OtherPlayer otherPlayer))
            {
                otherPlayer.ChangePkType(type);
            }
        }
    }

    public static void MonsterAttackInfo(Message msg)
    {
        int monsterId = msg.ReadInt();
        int dam = msg.ReadInt();
        byte count = msg.ReadByte();
        Monsters.TryGetValue(monsterId, out Monster attacker);
        for (int i = 0; i < count; i++)
        {
            int playerId = msg.ReadInt();
            BaseObject target = null;
            if (playerId == Player.Id)
            {
                target = Player;
                Player.AniTakeDamage(dam,attacker);
            }
            else
            {
                OtherPlayers.TryGetValue(playerId, out var value);
                target = value;
                if(value != null)
                {
                    value.AniTakeDamage(dam, attacker);
                }
            }
            if (target == null || target.IsDie())
            {
                return;
            }
            if (attacker != null && !attacker.IsDie())
            {
                attacker.AniAttack(target);
            }
        }
    }

    public static void PlayerWearingItems(Message msg)
    {
        int playerId = msg.ReadInt();
        byte size = msg.ReadByte();
        ItemEquipment[] items = new ItemEquipment[size];
        if(size != 3)
        {
            Debug.LogError("PlayerWearingItems quen sua roi kia");
            return;
        }
        for(int i = 0;i< size;i++)
        {
            bool has = msg.ReadBool();
            if (has)
            {
                string id = msg.ReadString();
                int templateId = msg.ReadInt();
                items[i] = new ItemEquipment(id, templateId);
            }
        }

        BasePlayer player = null;

        if(playerId == Player.Id)
        {
            player = Player;
        }
        else
        {
            OtherPlayers.TryGetValue(playerId, out var value);
            player = value;
        }
        if (player != null)
        {
            player.LoadPartWearing(items);
        }
    }

    public static void InventoryItems(Message msg)
    {
        byte size = msg.ReadByte();
        Player.InventoryItems = new BaseItem[size];
        for(int i = 0;i< size; i++)
        {
            BaseItem item = null;
            bool has = msg.ReadBool();
            if (has)
            {
                string id = msg.ReadString();
                int templateId = msg.ReadInt();
                ItemType type = (ItemType)msg.ReadByte();
                if (type == ItemType.Equipment)
                {
                    item = new ItemEquipment(id, templateId);
                }
                else
                {
                    int quantity = msg.ReadInt();
                    if(type == ItemType.Consumable)
                    {
                        item = new ItemConsumable(templateId, quantity);
                    }
                    else
                    {
                        item = new ItemMaterial(templateId, quantity);
                    }
                }
            }
            Player.InventoryItems[i] = item;
        }
    }
}
