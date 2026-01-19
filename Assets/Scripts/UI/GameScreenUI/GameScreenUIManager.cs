using Assets.Scripts.Networking;
using Assets.Scripts.Utils;
using System;
using TMPro;
using UnityEngine;

public class GameScreenUIManager : BaseUIManager
{
    [SerializeField] public HUDManager hudManager;
    [SerializeField] private MenuTabManager menuTabManager;
    [SerializeField] private TalkingUIManager talkingUIManager;
    [SerializeField] private ShopTabManager shopTabManager;
    [SerializeField] private SettingsPanelUIManager settingsPanelUIManager;

    [SerializeField] private TextMeshProUGUI txtCenterNotification;
    private static long startTimeShowTxtCenterNotification;

    private void Update()
    {
        // Xử lý ESC key để mở/đóng settings panel
        if (Input.GetKeyDown(KeyCode.Escape))
        {
     
            ToggleSetting();
        }

        if(SystemUtil.CurrentTimeMillis() - startTimeShowTxtCenterNotification > 2000)
        {
            if (ClientReceiveMessageHandler.CenterNotifications.TryDequeue(out var msg))
            {
                txtCenterNotification.text = msg;
                startTimeShowTxtCenterNotification = SystemUtil.CurrentTimeMillis();
            }
            else
            {
                txtCenterNotification.text = "";
            }
        }
    }

    public void ToggleSetting()
    {
        if (settingsPanelUIManager != null)
        {
            if (settingsPanelUIManager.gameObject.activeSelf)
            {
                settingsPanelUIManager.HideUI();
            }
            else
            {
                settingsPanelUIManager.ShowUI();
            }
        }
        else         {
            Debug.LogWarning("SettingsPanelUIManager is not assigned in GameScreenUIManager.");
        }
    }

    public override void ShowUI()
    {
        base.ShowUI();
        hudManager.ShowUI();
        menuTabManager.HideUI();
        talkingUIManager.HideUI();
        shopTabManager.HideUI();
        if (settingsPanelUIManager != null)
        {
            settingsPanelUIManager.HideUI();
        }
    }

    public void ShowMenuTab()
    {
        menuTabManager.ShowUI();
        hudManager.HideUI();
        talkingUIManager.HideUI();
        shopTabManager.HideUI();
    }

    public void ShowHUD()
    {
        hudManager.ShowUI();
        menuTabManager.HideUI();
        talkingUIManager.HideUI();
        shopTabManager.HideUI();
    }


    public void ShowTalkingUI()
    {
        hudManager.HideUI();
        menuTabManager.HideUI();
        talkingUIManager.ShowUI();
        shopTabManager.HideUI();
    }

    public void ShowShopTab()
    {
        hudManager.HideUI();
        menuTabManager.HideUI();
        talkingUIManager.HideUI();
        shopTabManager.ShowUI();
    }

    public void ShowTalking(BaseObject baseObject)
    {
        if (baseObject == null || !baseObject.IsNpc())
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
