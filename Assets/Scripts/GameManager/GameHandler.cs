using Assets.Scripts.Map;
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
    public static List<OtherPlayer> OtherPlayers = new List<OtherPlayer>();



 
    public static void PlayerEnterMap(short mapId, string mapName, short x, short y)
    {
        UIManager.Instance.EnableLoadScreen();


        //map
        MapManager.MapId = mapId;
        MapManager.MapName = mapName;

        //player
        Player.SetPosition(x, y);




        UIManager.Instance.DisableLoadScreen();
    }
}
