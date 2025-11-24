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


    public abstract void AutoMoveToXY(int x, int y);
}
