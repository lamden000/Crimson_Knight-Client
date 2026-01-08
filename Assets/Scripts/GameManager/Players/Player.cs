using Assets.Scripts.Networking;
using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;


public class Player : BaseObject
{
    public BaseObject objFocus;

    public PlayerMovementController PlayerMovementController;

    public LayerMask targetMask;
    private Transform arrowIndicator;


    private float lastClickTime = 0f;

    public static Player Create(int id, string name)
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
        targetMask = LayerMask.GetMask("Player", "Monster", "Npc");
    }

    public override void AutoMoveToXY(int x, int y)
    {

        PlayerMovementController.MoveToXY(x, y);
    }

    void Update()
    {
        UpdateTargetLogic();
        UpdateMouse();
        UpdateInput();
    }

    private void UpdateInput()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Attack(-1, objFocus);
        }
    }

    private void UpdateMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

            BaseObject nearestObject = FindNearestObjectToPosition(mouseWorldPos);

            if (nearestObject != null)
            {
                objFocus = nearestObject;
                SetFocus(objFocus);

                float timeSinceLastClick = Time.time - lastClickTime;
                if (timeSinceLastClick <= 0.3f)
                {
                    Attack(-1, objFocus);
                }
                lastClickTime = Time.time;
            }
        }
    }

    BaseObject FindNearestObjectToPosition(Vector3 position)
    {
        BaseObject result = null;
        float minDistSq = float.MaxValue;
        float maxClickDistance = 50f;

        void Check(BaseObject obj)
        {
            if (obj == null || obj.transform == null) return;

            float distSq = Vector3.SqrMagnitude(obj.transform.position - position);
            if (distSq < minDistSq && distSq <= maxClickDistance * maxClickDistance)
            {
                minDistSq = distSq;
                result = obj;
            }
        }

        foreach (var p in GameHandler.Npcs.Values)
            Check(p);

        foreach (var m in GameHandler.Monsters.Values)
            Check(m);

        foreach (var op in GameHandler.OtherPlayers.Values)
            Check(op);

        return result;
    }

    void UpdateTargetLogic()
    {
        int maxTargetDistance = 300;

        if (objFocus == null || MathUtil.Distance(this, objFocus) > maxTargetDistance)
        {
            BaseObject newTarget = FindNearestTarget();

            if (newTarget == null || MathUtil.Distance(this, newTarget) > maxTargetDistance)
            {
                LoseTarget();
                return;
            }
            objFocus = newTarget;
        }
        SetFocus(objFocus);
    }

    public void SetFocus(BaseObject objFocus)
    {
        if (objFocus == null)
        {
            return;
        }
        if (!arrowIndicator.gameObject.activeInHierarchy)
        {
            arrowIndicator.gameObject.SetActive(true);
        }

        float offsetY = objFocus.GetTopOffsetY();
        arrowIndicator.position = objFocus.transform.position + Vector3.up * offsetY;

        this.objFocus = objFocus;
    }


    BaseObject FindNearestTarget()
    {
        BaseObject result = null;
        int minDistSq = int.MaxValue;

        void Check(BaseObject obj)
        {
            int d = MathUtil.Distance(GameHandler.Player, obj);
            if (d < minDistSq)
            {
                minDistSq = d;
                result = obj;
            }
        }

        foreach (var p in GameHandler.Npcs.Values)
            Check(p);

        foreach (var m in GameHandler.Monsters.Values)
            Check(m);

        foreach (var op in GameHandler.OtherPlayers.Values)
            Check(op);

        return result;

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
        if (objFocus != null && objFocus.GetObjectType() == ObjectType.Npc)
        {
            UIManager.Instance.gameScreenUIManager.ShowTalking(GameHandler.Player.objFocus);
        }
    }

    public bool CanAttack()
    {
        if (this.objFocus == null)
        {
            return false;
        }

        if (this.objFocus.GetObjectType() == ObjectType.Npc)
        {
            return false;
        }

        return true;
    }
   
}
