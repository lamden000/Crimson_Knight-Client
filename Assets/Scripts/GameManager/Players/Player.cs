using Assets.Scripts.GameManager.Map;
using Assets.Scripts.GameManager.Players;
using Assets.Scripts.Networking;
using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;


public class Player : BasePlayer
{
    public BaseItem[] InventoryItems;
    public List<Skill> Skills = new List<Skill>();
    public int CurrentMp { get; set; }
    public int MaxMp { get; set; }

    public long Exp;
    public long Gold;

    public BaseObject objFocus;


    public LayerMask targetMask;
    private Transform arrowIndicator;


    private float lastClickTime = 0f;

    public static Player Create(int id, string name, ClassType classType, Gender gender)
    {
        GameObject gameObject = SpawnManager.GI().SpawnCharacterPrefab(0, 0);
        Player player = gameObject.AddComponent<Player>();
        player.ClassType = classType;
        player.Gender = gender;
        player.Id = id;
        player.Name = name;

        player.SetupPrefab(true);
        GameObject pkicon = SpawnManager.GI().SpawnPkIconPrefab();
        pkicon.transform.SetParent(player.transform);
        pkicon.transform.localPosition = new Vector3(0, player.GetTopOffsetY() - 5, 0);
        player.SetPkIcon(pkicon);
        return player;
    }

    void Start()
    {
        if (arrowIndicator == null)
            arrowIndicator = GameObject.FindGameObjectWithTag("Target Arrow").transform;
        targetMask = LayerMask.GetMask("Player", "Monster", "Npc");
    }


    void Update()
    {
        if (IsDie())
        {
            return;
        }
        UpdateTargetLogic();
        UpdateMouse();
        UpdateInput();
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            RequestManager.ChangePkType(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            RequestManager.ChangePkType((PkType)1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            RequestManager.ChangePkType((PkType)2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            RequestManager.ChangePkType((PkType)3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            RequestManager.ChangePkType((PkType)4);
        }

       
    }

    public string EffectName = "ConductedEffect";
    private void UpdateInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            SpawnManager.GI().SpawnEffectPrefab(EffectName, this.transform, objFocus.transform);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Attack(GetSkillIdCanAttack(), objFocus);
        }
    }

    private int GetSkillIdCanAttack()
    {
        foreach(var skill in Skills)
        {
            if (skill.IsLearned && skill.CanAttack())
            {
                return skill.TemplateId;
            }
        }
        return -1;
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
                    Attack(GetSkillIdCanAttack(), objFocus);
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

        foreach (var p in ClientReceiveMessageHandler.Npcs.Values)
            Check(p);

        foreach (var m in ClientReceiveMessageHandler.Monsters.Values)
        {
            if (m.IsDie())
            {
                continue;
            }
            Check(m);
        }

        foreach (var op in ClientReceiveMessageHandler.OtherPlayers.Values)
            Check(op);
        foreach (var item in ClientReceiveMessageHandler.ItemPicks.Values)
            Check(item);

        return result;
    }

    void UpdateTargetLogic()
    {
        int maxTargetDistance = 300;

        if (objFocus == null || MathUtil.Distance(this, objFocus) > maxTargetDistance || (objFocus.IsMonster() && objFocus.IsDie()))
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
        if (objFocus.IsOtherPlayer())
        {
            PlayerAnimationController playerAnimationController = objFocus.GetComponent<PlayerAnimationController>();
            if (playerAnimationController != null)
            {
                if (playerAnimationController.currentDir == Direction.Down || playerAnimationController.currentDir == Direction.Up)
                {
                    offsetY -= 10;
                }
                else
                {
                    offsetY += 5;
                }
            }
        }

        arrowIndicator.position = objFocus.transform.position + new Vector3(0, offsetY);
        this.objFocus = objFocus;
    }


    BaseObject FindNearestTarget()
    {
        BaseObject result = null;
        int minDistSq = int.MaxValue;

        void Check(BaseObject obj)
        {
            int d = MathUtil.Distance(ClientReceiveMessageHandler.Player, obj);
            if (d < minDistSq)
            {
                minDistSq = d;
                result = obj;
            }
        }

        foreach (var p in ClientReceiveMessageHandler.Npcs.Values)
            Check(p);

        foreach (var m in ClientReceiveMessageHandler.Monsters.Values)
        {
            if (m.IsDie())
            {
                continue;
            }
            Check(m);
        }

        foreach (var op in ClientReceiveMessageHandler.OtherPlayers.Values)
            Check(op);
        foreach(var item in ClientReceiveMessageHandler.ItemPicks.Values)
            Check(item);

        return result;

    }

    void LoseTarget()
    {
        this.objFocus = null;
        arrowIndicator.gameObject.SetActive(false);
    }
    public override bool IsPlayer()
    {
        return true;
    }

    private static long startTimePickItem = 0;
    public void Attack(int skillId, BaseObject target)
    {
        if (target == null)
        {
            return;
        }
        if (target.IsNpc())
        {
            UIManager.Instance.gameScreenUIManager.ShowTalking(target);
        }
        else if (target.IsItemPick())
        {
            if(SystemUtil.CurrentTimeMillis() - startTimePickItem > 500)
            {
                Debug.Log("item pick " + ((ItemPick)target).IdItemPick);
                startTimePickItem = SystemUtil.CurrentTimeMillis();
                RequestManager.PickItem(((ItemPick)target).IdItemPick);
            }
            else
            {
                ClientReceiveMessageHandler.CenterNotifications.Enqueue("Thao tác quá nhanh");
            }
        }
        else
        {
            Debug.Log("objfocus: " + target.GetX() + "-" + target.GetY());
            Skill skillUse = Skills[skillId];
            if (skillUse != null && skillUse.CanAttack())
            {
                if (!skillUse.IsLearned)
                {
                    Debug.Log("chua hoc skill");
                    return;
                }
                if (this.CurrentMp <= skillUse.GetMpLost())
                {
                    Debug.Log("khong du mp");
                    return;
                }
                int targetCount = skillUse.GetTargetCount();
                int range = skillUse.GetRange();

                List<BaseObject> targets = new List<BaseObject>();
                if (target.IsOtherPlayer())
                {
                    OtherPlayer otherPlayer = (OtherPlayer)target;
                    if (otherPlayer.PkType == this.PkType)
                    {
                        Debug.Log("cung type pk");
                        return;
                    }
                }
                targets.Add(target);
                int remainSlot = targetCount - targets.Count;
                if (remainSlot > 0)
                {
                    int ranPlayer = MathUtil.RandomInt(0, remainSlot);
                    foreach (var otherPlayer in ClientReceiveMessageHandler.OtherPlayers.Values)
                    {
                        if (ranPlayer <= 0)
                            break;

                        if (otherPlayer.IsDie())
                            continue;

                        if (otherPlayer.PkType == this.PkType)
                            continue;

                        int dist = MathUtil.Distance(this, otherPlayer);
                        if (dist > range)
                            continue;

                        if (targets.Any(t => t.Id == otherPlayer.Id))
                            continue;

                        targets.Add(otherPlayer);
                        ranPlayer--;
                    }

                    foreach (var monster in ClientReceiveMessageHandler.Monsters.Values)
                    {
                        if (targets.Count >= targetCount)
                            break;

                        if (monster.IsDie())
                            continue;

                        int dist = MathUtil.Distance(this, monster);
                        if (dist > range)
                            continue;

                        if (targets.Any(t => t.Id == monster.Id))
                            continue;

                        targets.Add(monster);
                    }
                }

                if (targets.Count == 0)
                {
                    Debug.Log("khong co target trong range");
                    return;
                }
                bool[] isPlayers = new bool[targets.Count];
                int[] targetIds = new int[targets.Count];
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i].IsOtherPlayer())
                    {
                        isPlayers[i] = true;
                    }
                    else
                    {
                        isPlayers[i] = false;
                    }
                    targetIds[i] = targets[i].Id;
                }
                RequestManager.RequestAttack(skillUse.TemplateId, isPlayers, targetIds);
                skillUse.StartTimeAttack = SystemUtil.CurrentTimeMillis();
                this.CurrentMp -= skillUse.GetMpLost();
                Debug.Log("Send attack " + skillUse.TemplateId);

                foreach(var t in targets)
                {
                    SpawnManager.GI().SpawnEffectPrefab(skillUse.GetTemplate().EffectName, this.transform, t.transform);
                }
                AniAttack(target);
            }
            else
            {
                Debug.Log("chua hoi chieu");
            }
        }
    }



    public void LoadPlayerSkillInfoFromServer(Message msg)
    {
        //skills
        byte size = msg.ReadByte();
        Skills.Clear();
        for (int i = 0; i < size; i++)
        {
            int templateId = msg.ReadInt();
            sbyte variant = msg.ReadSByte();
            Skill skill = new Skill(templateId, variant, this.ClassType);
            Skills.Add(skill);
        }
    }

    public BaseItem GetItemInventoty(int templateId, ItemType type)
    {
        foreach(var item in this.InventoryItems)
        {
            if(item != null)
            {
                if(item.TemplateId == templateId && item.GetItemType() == type)
                {
                    return item;
                }
            }
        }
        return null;
    }
}
