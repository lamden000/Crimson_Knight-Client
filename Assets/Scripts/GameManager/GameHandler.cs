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
        Player = Player.SetUp();
    }



    public static Player Player;
    public static Dictionary<int, OtherPlayer> OtherPlayers = new Dictionary<int,OtherPlayer>();



 
    public static void PlayerEnterMap(Message msg)
    {
        UIManager.Instance.EnableLoadScreen();
        //map
        MapManager.MapId = msg.ReadShort();
        MapManager.MapName = msg.ReadString();
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
            OtherPlayer player = OtherPlayer.SetUp(id,name,xO,yO);
            OtherPlayers.TryAdd(id,player);
        }

        //player
        short x = msg.ReadShort();
        short y = msg.ReadShort();

        Player.gameObject.SetActive(true);
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
            other.AutoMoveToXY(x,y);
        }
        else
        {
            Debug.LogWarning("Khong tim thay other player co id la " + id);
        }
    }
}
