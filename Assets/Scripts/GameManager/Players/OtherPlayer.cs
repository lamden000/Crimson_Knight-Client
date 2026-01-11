using Assets.Scripts.GameManager.Players;
using Assets.Scripts.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class OtherPlayer : BasePlayer
{

    public Queue<Tuple<int,int>> Moves = new Queue<Tuple<int,int>>();

    public static OtherPlayer Create(int id, string name, short x, short y,ClassType classType, Gender gender)
    {
        GameObject gameObject = SpawnManager.GI().SpawnCharacterPrefab(x, y);
        OtherPlayer otherPlayer = gameObject.AddComponent<OtherPlayer>();
        otherPlayer.Id = id;
        otherPlayer.Name = name;
        otherPlayer.ClassType = classType;
        otherPlayer.Gender = gender;
        otherPlayer.SetupPrefab();
       
        otherPlayer.SetPosition(x, y);
        GameObject nameTag = SpawnManager.GI().SpawnDisplayBaseObjectNamePrefab(otherPlayer.Name);
        nameTag.transform.SetParent(otherPlayer.transform);
        nameTag.transform.localPosition = new Vector3(0, otherPlayer.GetTopOffsetY() - 5, 0);
        otherPlayer.SetNameTag(nameTag);

        GameObject pkicon = SpawnManager.GI().SpawnPkIconPrefab();
        pkicon.transform.SetParent(otherPlayer.transform);
        pkicon.transform.localPosition = new Vector3(0, otherPlayer.GetTopOffsetY() + 15, 0);
        otherPlayer.SetPkIcon(pkicon);
        return otherPlayer;
    }



   
    private void FixedUpdate()
    {
        if(Moves.TryDequeue(out Tuple<int,int> move))
        {
            this.AutoMoveToXY(move.Item1, move.Item2);
        }
    }


    public override bool IsOtherPlayer()
    {
        return true;
    }
    
}
