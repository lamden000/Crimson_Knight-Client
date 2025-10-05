using System;
using UnityEngine;

public class Character : MonoBehaviour
{
    public enum Clazz { Knight, Assassin, Markman, Wizard }

    [SerializeField]
    private Clazz m_Class;

    public Clazz GetClass()
    { return m_Class; }

    public CharacterPart getWeaponType()
    {
        CharacterPart weapon = CharacterPart.Gun;
        switch (m_Class)
        {
            case Clazz.Assassin:
                weapon = CharacterPart.Knive;
                break;
            case Clazz.Knight:
                weapon = CharacterPart.Sword;
                break;
            case Clazz.Wizard:
                weapon = CharacterPart.Staff;
                break;
            case Clazz.Markman:
                weapon = CharacterPart.Gun;
                break;
        }

        return weapon;
    }    
}
