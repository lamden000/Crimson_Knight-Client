using Assets.Scripts;
using Assets.Scripts.Map;
using Assets.Scripts.Networking;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static ScreenType CurrentScreenType = ScreenType.LoginScreen;
    public static UIManager Instance { get; private set; }

    [SerializeField] private DialogFactory dialogFactory;

    [SerializeField] private LoginScreenUIManager loginScreenUIManager;
    [SerializeField] public GameScreenUIManager gameScreenUIManager;
    [SerializeField] private LoadScreenUIManager loadScreenUIManager;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log("[UIManager] Awake");
    }

    public static void Init()
    {
        Debug.Log("[UIManager] Init");

        UIManager.Instance.DisableLoadScreen();
        UIManager.Instance.DisableGameScreen();
        UIManager.Instance.EnableLoginScreen();
    }

    #region Dialog
    public void ShowYesNo(string message, int idDialog, Action<bool, int> callback)
    {
        Debug.Log("[UIManager] ShowYesNo");
        var dialog = dialogFactory.CreateYesNo();
        dialog.Setup(message, idDialog, callback);
        dialog.Show();
    }

    public void ShowDropdown(string title, string[] options, Action<int> callback)
    {
        Debug.Log("[UIManager] ShowDropdown");
        var dialog = dialogFactory.CreateDropdown();
        dialog.Setup(title, options, callback);
        dialog.Show();
    }

    public void ShowOK(string message, Action onOkClicked = null)
    {
        Debug.Log("[UIManager] ShowOK");
        var dialog = dialogFactory.CreateOK();
        dialog.Setup(message, onOkClicked);
        dialog.Show();
    }
    #endregion

    #region Screen Control

    public void EnableLoginScreen()
    {
        UIManager.CurrentScreenType = ScreenType.LoginScreen;

        this.loginScreenUIManager.ShowUI();
        this.loadScreenUIManager.HideUI();
        this.gameScreenUIManager.HideUI();

        Debug.Log("[UIManager] EnableLoginScreen");
        Debug.Log("[UIManager] Play Login Music");

        AudioManager.Instance?.PlayLoginMusic();
    }

    public void DisableLoginScreen()
    {
        this.loginScreenUIManager.HideUI();
        Debug.Log("[UIManager] DisableLoginScreen");
    }

    public void EnableLoadScreen()
    {
        UIManager.CurrentScreenType = ScreenType.LoadScreen;

        Debug.Log(loginScreenUIManager.gameObject.name);
        this.loginScreenUIManager.HideUI();
        this.loadScreenUIManager.ShowUI();
        this.gameScreenUIManager.HideUI();

        Debug.Log("[UIManager] EnableLoadScreen (keep current music)");
    }

    public void DisableLoadScreen()
    {
        this.loadScreenUIManager.HideUI();
        Debug.Log("[UIManager] DisableLoadScreen");
    }

    public void EnableGameScreen()
    {
        this.gameScreenUIManager.ShowUI();
        this.loginScreenUIManager.HideUI();
        this.loadScreenUIManager.HideUI();

        Debug.Log("[UIManager] EnableGameScreen");
        Debug.Log("[UIManager] Play Game Music");

        AudioManager.Instance?.PlayGameMusic();
    }

    public void DisableGameScreen()
    {
        this.gameScreenUIManager.HideUI();
        Debug.Log("[UIManager] DisableGameScreen");
    }
    #endregion


}
