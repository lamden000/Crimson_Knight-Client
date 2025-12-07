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
    public int CurrentHp { get; set; }
    public int MaxHp { get; set; }
    public int CurrentMp { get; set; }
    public int MaxMp { get; set; }

    protected BaseObject currentTarget;

    public abstract void AutoMoveToXY(int x, int y);
    public abstract void DestroyObject();

    public void SetPosition(short x, short y)
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

    public void SetTarget(BaseObject target)
    {
        currentTarget = target;
    }
}
