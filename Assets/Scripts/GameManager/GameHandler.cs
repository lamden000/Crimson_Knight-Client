using Assets.Scripts.Map;
using Assets.Scripts.Networking;
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
    public static Dictionary<int, OtherPlayer> OtherPlayers = new Dictionary<int,OtherPlayer>();



 
    public static void PlayerEnterMap(Message msg)
    {
        UIManager.Instance.EnableLoadScreen();
        //map
        MapManager.MapId = msg.ReadShort();
        MapManager.MapName = msg.ReadString();
        foreach(var obj in OtherPlayers)
        {
            obj.Value.DestroyObject();
        }
        OtherPlayers.Clear();

        int size = msg.ReadShort();
        for(int i = 0;i< size; i++)
        {
            int id = msg.ReadInt();
            string name = msg.ReadString();
            short xO = msg.ReadShort();
            short yO = msg.ReadShort();
            if (id == Player.Id)
            {
                continue;
            }
            OtherPlayer player = OtherPlayer.Create(id,name,xO,yO);
            if (!OtherPlayers.TryAdd(id, player))
            {
                player.DestroyObject();
            }
        }

        //player
        short x = msg.ReadShort();
        short y = msg.ReadShort();

        Player.SetPosition(x, y);




        UIManager.Instance.DisableLoadScreen();
    }

    public static void OtherPlayerMove(Message msg)
    {
        int id = msg.ReadInt();
        int x = msg.ReadShort();
        int y = msg.ReadShort();
        if (OtherPlayers.TryGetValue(id,out OtherPlayer other))
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
}
