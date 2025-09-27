using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public enum CharacterPart { Legs = 0, Body = 1, Head = 2, Hat = 3, Eyes=4, Hair=5,Wings=6, Weapon=50 }
public enum BodyState { IdleDown = 0, IdleUp = 5, IdleLeft = 10, WalkLeft_1 = 12, WalkLeft_2 = 3, WalkUp_1=9, WalkUp_2 = 13,WalkDown_1=4, WalkDown_2 = 8, AttackUp=2, AttackDown = 12, AttackLeft = 7 }
public enum HeadState { Down=0, Up=1, Left=2}