using Assets.Scripts.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class OtherPlayer : BaseObject
{
    public ClassType ClassType;



    public PlayerMovementController PlayerMovementController;

    public Queue<Tuple<int,int>> Moves = new Queue<Tuple<int,int>>();

    public static OtherPlayer Create(int id, string name, short x, short y)
    {
        GameObject gameObject = SpawnManager.GI().SpawnOtherCharacterPrefab(x, y);
        OtherPlayer otherPlayer = gameObject.AddComponent<OtherPlayer>();
        otherPlayer.PlayerMovementController = otherPlayer.gameObject.GetComponent<PlayerMovementController>();
        otherPlayer.PlayerMovementController.IsMainPlayer = false;

        //
        otherPlayer.Id = id;
        otherPlayer.Name = name;
        otherPlayer.SetPosition(x, y);
        GameObject nameTag = SpawnManager.GI().SpawnDisplayBaseObjectNamePrefab(otherPlayer.Name);
        nameTag.transform.SetParent(otherPlayer.transform);
        nameTag.transform.localPosition = new Vector3(0, otherPlayer.GetTopOffsetY(), 0);
        otherPlayer.SetNameTag(nameTag);
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

    public override ObjectType GetObjectType()
    {
       return ObjectType.OtherPlayer;
    }

    public void Attack(int skillId, BaseObject target)
    {
        //
    }

    public override float GetTopOffsetY()
    {
        return base.GetTopOffsetY() + 28;
    }

    public override void LoadBaseInfoFromServer(Message msg)
    {
        this.CurrentHp = msg.ReadInt();
        this.MaxHp = msg.ReadInt();
        this.Level = msg.ReadShort();
    }
}
