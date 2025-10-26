using Assets.Scripts.Networking;
using Assets.Scripts.Utils;
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

    public void TakeDamage(int damage, GameObject attacker)
    {

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Session.Connect();
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Vector3 targetPosition = new Vector3(1034.6f, 481.5147f, transform.position.z);
                transform.position = targetPosition;
            }
        }
    }


  
}
