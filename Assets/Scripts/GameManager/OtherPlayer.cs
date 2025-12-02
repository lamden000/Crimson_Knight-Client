using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class OtherPlayer : BaseObject
{
    public static OtherPlayer SetUp()
    {
        GameObject gameObject = SpawnManager.GI().SpawnCharacter(492, 492);
        OtherPlayer otherPlayer = gameObject.AddComponent<OtherPlayer>();
        otherPlayer.PlayerMovementController = otherPlayer.gameObject.GetComponent<PlayerMovementController>();
        otherPlayer.PlayerMovementController.IsMainPlayer = false;
        GameHandler.OtherPlayers.Add(otherPlayer);
        return otherPlayer;
    }


    public PlayerMovementController PlayerMovementController;

    public override void AutoMoveToXY(int x, int y)
    {
        PlayerMovementController.MoveToXY(x, y);
    }
}
