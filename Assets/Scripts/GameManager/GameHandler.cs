using Assets.Scripts.Map;
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
        MapManager.Initialize();
        MapManager.LoadMapForLoginScreen();
    }


    public static Player Player { get; set; }
    public static List<OtherPlayer> OtherPlayers = new List<OtherPlayer>();


}
