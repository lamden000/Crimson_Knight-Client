using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class OtherPlayer : BaseObject
{
    public static OtherPlayer SetUp(int id, string name, short x, short y)
    {
        GameObject gameObject = SpawnManager.GI().SpawnCharacter(x, y);
        OtherPlayer otherPlayer = gameObject.AddComponent<OtherPlayer>();
        otherPlayer.PlayerMovementController = otherPlayer.gameObject.GetComponent<PlayerMovementController>();
        otherPlayer.PlayerMovementController.IsMainPlayer = false;
        return otherPlayer;
    }


    public PlayerMovementController PlayerMovementController;

    public override void AutoMoveToXY(int x, int y)
    {
        PlayerMovementController.MoveToXY(x, y);
    }
}
