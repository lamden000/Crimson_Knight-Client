using Assets.Scripts.Map;
using Assets.Scripts.Networking;
using Assets.Scripts.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameHandler : MonoBehaviour
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


    public static void PlayerEnterMap(Message msg)
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

    public static void OtherPlayerMove(Message msg)
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

    public static void OtherPlayerEnterMap(int otherPlayerId, string otherPlayerName, short otherX, short otherY)
    {
        OtherPlayer player = OtherPlayer.Create(otherPlayerId, otherPlayerName, otherX, otherY);
        if (!OtherPlayers.TryAdd(otherPlayerId, player))
        {
            player.DestroyObject();
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


    public static void MonsterAttack(BaseObject objAttack, BaseObject objTarget)
    {
        if (objAttack.GetObjectType() == ObjectType.Monster)
        {
            if (objTarget.GetObjectType() == ObjectType.Player)
            {
                Monster monster = (Monster)objAttack;
                Player player = (Player)objTarget;
            }
            else if (objTarget.GetObjectType() == ObjectType.OtherPlayer)
            {
                OtherPlayer otherPlayer = (OtherPlayer)objTarget;
                Monster monster = (Monster)objAttack;
            }

        }
    }

    public static void OtherPlayerAttack(int skillId, int otherPlayerId, BaseObject objTarget)
    {
        if (OtherPlayers.TryGetValue(otherPlayerId, out OtherPlayer other))
        {
            other.Attack(skillId, objTarget);
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
            short xO = msg.ReadShort();
            short yO = msg.ReadShort();
            if (id == Player.Id)
            {
                continue;
            }
            OtherPlayer player = OtherPlayer.Create(id, name, xO, yO);
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
}
