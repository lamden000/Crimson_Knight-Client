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
    public PkType PkType;


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

        GameObject pkicon = SpawnManager.GI().SpawnPkIconPrefab();
        pkicon.transform.SetParent(otherPlayer.transform);
        pkicon.transform.localPosition = new Vector3(0, otherPlayer.GetTopOffsetY() + 15, 0);
        otherPlayer.SetPkIcon(pkicon);
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

    public void ChangePkType(PkType type)
    {
        this.PkType = type;
        PkIconManager pkIconManager = PkIcon.GetComponent<PkIconManager>();
        if (pkIconManager != null)
        {
            pkIconManager.SetPkState(this.PkType);
        }
    }

    private GameObject PkIcon;
    private Vector3 PkIconOriginalScale;
    private Quaternion PkIconOriginalRotation;
    public void SetPkIcon(GameObject icon)
    {
        this.PkIcon = icon;
        PkIconOriginalScale = PkIcon.transform.localScale;
        PkIconOriginalRotation = PkIcon.transform.localRotation;
    }


    protected override void LateUpdate()
    {
        base.LateUpdate();
        if (PkIcon != null)
        {
            Vector3 targetScale = PkIconOriginalScale;
            Quaternion targetRotation = PkIconOriginalRotation;

            if (transform.localScale.x < 0)
            {
                targetScale.x = -Mathf.Abs(PkIconOriginalScale.x);
            }
            else
            {
                targetScale.x = Mathf.Abs(PkIconOriginalScale.x);
            }

            float yRotation = transform.rotation.eulerAngles.y;
            if (Mathf.Abs(yRotation - 180f) < 1f)
            {
                targetRotation = Quaternion.Euler(0, 180f, 0) * PkIconOriginalRotation;
            }

            PkIcon.transform.localScale = targetScale;
            PkIcon.transform.localRotation = targetRotation;
        }
    }
}
