using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;


public class OtherPlayer : BaseObject
{
    public PlayerMovementController PlayerMovementController;

    public Queue<Tuple<int,int>> Moves = new Queue<Tuple<int,int>>();

    public static OtherPlayer Create(int id, string name, short x, short y)
    {
        GameObject gameObject = SpawnManager.GI().SpawnOtherCharacter(x, y);
        OtherPlayer otherPlayer = gameObject.AddComponent<OtherPlayer>();
        otherPlayer.PlayerMovementController = otherPlayer.gameObject.GetComponent<PlayerMovementController>();
        otherPlayer.PlayerMovementController.IsMainPlayer = false;

        //
        otherPlayer.Id = id;
        otherPlayer.Name = name;
        otherPlayer.SetPosition(x, y);
        return otherPlayer;
    }



    public override void AutoMoveToXY(int x, int y)
    {
    }
    private void FixedUpdate()
    {
        if(Moves.TryDequeue(out Tuple<int,int> move))
        {
            PlayerMovementController.MoveToXY(move.Item1, move.Item2);
        }
    }
    public override void DestroyObject()
    {
        Destroy(this.gameObject);
    }



}
