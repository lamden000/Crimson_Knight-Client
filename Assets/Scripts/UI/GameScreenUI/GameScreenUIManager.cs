using Assets.Scripts.Networking;
using System;
using UnityEngine;

public class GameScreenUIManager : BaseUIManager
{
    [SerializeField] private HUDManager hudManager;
    [SerializeField] private MenuTabManager menuTabManager;
    [SerializeField] private TalkingUIManager talkingUIManager;
    public override void ShowUI()
    {
        base.ShowUI();
        hudManager.ShowUI();
        menuTabManager.HideUI();
        talkingUIManager.HideUI();
    }

    public void ShowMenuTab()
    {
        menuTabManager.ShowUI();
        hudManager.HideUI();
        talkingUIManager.HideUI();
    }

    public void ShowHUD()
    {
        hudManager.ShowUI();
        menuTabManager.HideUI();
        talkingUIManager.HideUI();
    }


    public void ShowTalkingUI()
    {
        hudManager.HideUI();
        menuTabManager.HideUI();
        talkingUIManager.ShowUI();
    }
    public void ShowTalking(BaseObject baseObject)
    {
        if (baseObject == null || baseObject.GetObjectType() != ObjectType.Npc)
        {
            return;
        }

        Npc npc = (Npc)baseObject;
        Action actionYes = () =>
        {
            RequestManager.RequestShowMenu(npc.Template.Id);
            Debug.Log(npc.Template.Name);
            talkingUIManager.HideUI();
            ShowHUD();
        };
        Action actionNo = () =>
        {
            talkingUIManager.HideUI();
            ShowHUD();
        };
        talkingUIManager.Setup(npc.Template.Name, npc.Template.Content, actionYes, actionNo);
        ShowTalkingUI();
    }
}
