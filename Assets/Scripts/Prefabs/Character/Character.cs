using Assets.Scripts.Networking;
using Assets.Scripts.Utils;
using System;
using UnityEngine;

public class Character : MonoBehaviour
{

    [SerializeField]
    private ClassType m_Class;

    private PlayerMovementController m_Controller;

    private void Start()
    {
        m_Controller = GetComponent<PlayerMovementController>();
        m_Class = ClassType.XA_THU;
    }

    public CharacterPart getWeaponType()
    {
        CharacterPart weapon = CharacterPart.Gun;
        switch (m_Class)
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

    public void TakeDamage(float damage, GameObject attacker)
    {
        m_Controller.HandleGetHit();        
    }

    void Update()
    {
      
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
