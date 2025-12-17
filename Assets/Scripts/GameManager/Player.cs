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
    public float maxTargetDistance = 500f;
    public LayerMask targetMask;
    private Transform arrowIndicator;

    public static Player Create(int id,string name)
    {
        GameObject gameObject = SpawnManager.GI().SpawnCharacterPrefab(0, 0);
        Player player = gameObject.AddComponent<Player>();
        player.PlayerMovementController = player.gameObject.GetComponent<PlayerMovementController>();
        player.PlayerMovementController.IsMainPlayer = true;
        player.Id = id;
        player.Name = name;
        return player;
    }

    void Start()
    {
        if (arrowIndicator == null)
            arrowIndicator = GameObject.FindGameObjectWithTag("Target Arrow").transform;
        targetMask = LayerMask.GetMask("Player","Monster", "NPC");
    }

    public override void AutoMoveToXY(int x, int y)
    {

        PlayerMovementController.MoveToXY(x, y);
    }

    public override void DestroyObject()
    {

    }

    void Update()
    {
       UpdateTargetLogic();
    }

    void UpdateTargetLogic()
    {
        // 1️⃣ CHƯA CÓ TARGET -> TÌM LUÔN
        if (objFocus == null)
        {
            BaseObject newTarget = FindNearestTarget();
            if (newTarget != null)
                SetFocus(newTarget);
            return;
        }

        // 2️⃣ CÓ TARGET -> CHECK KHOẢNG CÁCH
        float dist = Vector3.Distance(transform.position, objFocus.transform.position);

        if (dist > maxTargetDistance)
        {
            BaseObject newTarget = FindNearestTarget();

            if (newTarget != null)
                SetFocus(newTarget);
            else
                LoseTarget();
        }      
        else
        {
            int characterArrowOffsetY = 100;
            
            if(objFocus.GetObjectType()==ObjectType.Monster)
            {
                characterArrowOffsetY = 30;
            }

            arrowIndicator.position = objFocus.transform.position + Vector3.up * characterArrowOffsetY;
        }
    }

    public override void SetFocus(BaseObject objFocus)
    {
        if(!arrowIndicator.gameObject.activeInHierarchy)
        {
            arrowIndicator.gameObject.SetActive(true);
        }
        base.SetFocus(objFocus);
    }

    BaseObject FindNearestTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, maxTargetDistance, targetMask);

        BaseObject nearest = null;
        float bestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            BaseObject bo = hit.GetComponent<BaseObject>();
            if (bo == null|| bo == this.GetComponent<BaseObject>()) continue;
            float d = Vector3.Distance(transform.position, bo.transform.position);

            if (d < bestDist)
            {
                bestDist = d;
                nearest = bo;
            }
        }

        return nearest;
    }

    void LoseTarget()
    {
        SetFocus(null);
        arrowIndicator.gameObject.SetActive(false);
    }
    public override ObjectType GetObjectType()
    {
        return ObjectType.Player;
    }

    public void Attack(int skillId, BaseObject target)
    {
        //
    }
}
