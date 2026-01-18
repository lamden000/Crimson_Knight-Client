using Assets.Scripts.GameManager.Players;
using Assets.Scripts.Networking;
using Assets.Scripts.Utils;
using System;
using UnityEngine;

public class Character : MonoBehaviour
{
    private ClassType classType;

    private PlayerMovementController m_Controller;

    private void Start()
    {
        m_Controller = GetComponent<PlayerMovementController>();
    }

    public void SetUp(ClassType classType)
    {
        this.classType = classType;
    }

    public CharacterPart getWeaponType()
    {
        CharacterPart weapon = CharacterPart.Gun;
        switch (this.classType)
        {
            case ClassType.SAT_THU:
                weapon = CharacterPart.Knive;
                break;
            case ClassType.CHIEN_BINH:
                weapon = CharacterPart.Sword;
                break;
            case ClassType.PHAP_SU:
                weapon = CharacterPart.Staff;
                break;
            case ClassType.XA_THU:
                weapon = CharacterPart.Gun;
                break;
        }

        return weapon;
    }

    public void AniTakeDamage()
    {
        if (m_Controller == null)
        {
            return;
        }
        m_Controller.HandleGetHit();        
    }

  
}
