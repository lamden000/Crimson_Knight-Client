using Assets.Scripts.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;


public class Player : BaseObject
{
    public PlayerMovementController PlayerMovementController;
    public static Player Create(int id,string name)
    {
        GameObject gameObject = SpawnManager.GI().SpawnCharacter(0, 0);
        Player player = gameObject.AddComponent<Player>();
        player.PlayerMovementController = player.gameObject.GetComponent<PlayerMovementController>();
        player.PlayerMovementController.IsMainPlayer = true;
        CameraFollow.GI().target = player.transform;
        //
        player.Id = id;
        player.Name = name;
        return player;
    }

    public override void AutoMoveToXY(int x, int y)
    {

        PlayerMovementController.MoveToXY(x, y);
    }

    public override void DestroyObject()
    {

    }
}
