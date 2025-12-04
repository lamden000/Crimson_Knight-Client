using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;


public class Player : BaseObject
{

    public static Player SetUp()
    {
        Player player = null;
        if (GameHandler.Player == null)
        {
            GameObject gameObject = SpawnManager.GI().SpawnCharacter(492, 492);
            gameObject.SetActive(false);
            player = gameObject.AddComponent<Player>();
            player.PlayerMovementController = player.gameObject.GetComponent<PlayerMovementController>();
            player.PlayerMovementController.IsMainPlayer = true;
            CameraFollow.GI().target = gameObject.transform;
            GameHandler.Player = player;
        }
        else
        {
            player = GameHandler.Player;
        }
        return player;
    }


    public PlayerMovementController PlayerMovementController;

    public override void AutoMoveToXY(int x, int y)
    {
        PlayerMovementController.MoveToXY(x, y);
    }
}
