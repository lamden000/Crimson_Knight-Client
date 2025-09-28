using System;
using UnityEngine;

public class Character : MonoBehaviour
{
    public enum Class { Knight, Assassin, Markman, Wizard }

    [SerializeField]
    private Class m_Class;

    public Class GetClass()
    { return m_Class; }
}
