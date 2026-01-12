using Assets.Scripts.Networking;
using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public abstract class BaseObject : MonoBehaviour
{
    public int Id { get; set; }
    public string Name { get; set; }
    private int _currentHp;

    public int CurrentHp
    {
        get => _currentHp;
        set
        {
            if (_currentHp == value) return;
            _currentHp = Math.Max(0, value);
            CheckCurrentHp();
        }
    }

    protected virtual void CheckCurrentHp()
    {
    }

    public int MaxHp { get; set; }
    public short Level { get; set; }

    // bo tro cho nametag
    protected GameObject nameTag;
    private Vector3 nameTagOriginalScale;
    private Quaternion nameTagOriginalRotation;

    public abstract void AutoMoveToXY(int x, int y);
    public virtual void DestroyObject()
    {
        Destroy(this.gameObject);
    }

    public virtual void SetPosition(short x, short y)
    {
        this.transform.position = new Vector3(x, y, this.transform.position.z);
    }
    public short GetX()
    {
        return (short)this.transform.position.x;
    }
    public short GetY()
    {
        return (short)this.transform.position.y;
    }
   
    public void SetEffect(int effectId, int duration)
    {
    }


    //public abstract ObjectType GetObjectType();

    public virtual float GetTopOffsetY()
    {
        Renderer objRenderer = this.GetComponentInChildren<Renderer>();
        if (objRenderer != null)
        {
            float offsetToTop = objRenderer.bounds.max.y - this.transform.position.y;
            return offsetToTop + 10f;
        }
        return 1.5f;
    }

    public void SetNameTag(GameObject nameTagObject)
    {
        nameTag = nameTagObject;
        if (nameTag != null)
        {
            nameTagOriginalScale = nameTag.transform.localScale;
            nameTagOriginalRotation = nameTag.transform.localRotation;
        }
    }

    protected virtual void LateUpdate()
    {
        if (nameTag != null)
        {
            if(this == ClientReceiveMessageHandler.Player.objFocus)
            {
                nameTag.SetActive(false);
            }
            else
            {
                nameTag.SetActive(true);
            }

            Vector3 targetScale = nameTagOriginalScale;
            Quaternion targetRotation = nameTagOriginalRotation;

            if (transform.localScale.x < 0)
            {
                targetScale.x = -Mathf.Abs(nameTagOriginalScale.x);
            }
            else
            {
                targetScale.x = Mathf.Abs(nameTagOriginalScale.x);
            }

            float yRotation = transform.rotation.eulerAngles.y;
            if (Mathf.Abs(yRotation - 180f) < 1f)
            {
                targetRotation = Quaternion.Euler(0, 180f, 0) * nameTagOriginalRotation;
            }

            nameTag.transform.localScale = targetScale;
            nameTag.transform.localRotation = targetRotation;
        }
    }

    public bool IsDie()
    {
        return CurrentHp <= 0;
    }

    public virtual void AniAttack(BaseObject target = null) { }
    public virtual void AniTakeDamage(int dam, BaseObject attacker) { }


    public virtual bool IsMonster() { return false; }
    public virtual bool IsNpc() { return false; }
    public virtual bool IsPlayer() { return false; }
    public virtual bool IsOtherPlayer() { return false; }
    public virtual bool IsItemPick() {  return false; }
}

