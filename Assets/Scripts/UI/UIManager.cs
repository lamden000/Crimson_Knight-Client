using Assets.Scripts;
using Assets.Scripts.Map;
using Assets.Scripts.Networking;
using System;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static ScreenType CurrentScreenType = ScreenType.LoginScreen;


    public static UIManager Instance { get; private set; }

    [SerializeField] private DialogFactory dialogFactory;
    public Canvas LoginScreenCanvas;
    public Canvas LoadScreenCanvas;
    public GameObject GameScreenCanvas;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        btnLogin.onClick.AddListener(onClickLogin);

    }

    public static void Init()
    {
        UIManager.Instance.DisableLoadScreen();
        UIManager.Instance.DisableGameScreen();
        UIManager.Instance.EnableLoginScreen();
    }

    public void ShowYesNo(string message, int idDialog, System.Action<bool, int> callback)
    {
        var dialog = dialogFactory.CreateYesNo();
        dialog.Setup(message, idDialog, callback);
        dialog.Show();
    }


    public void ShowDropdown(string title, string[] options, System.Action<int> callback)
    {
        var dialog = dialogFactory.CreateDropdown();
        dialog.Setup(title, options, callback);
        dialog.Show();
    }
    public void ShowOK(string message, System.Action onOkClicked = null)
    {
        var dialog = dialogFactory.CreateOK();
        dialog.Setup(message, onOkClicked);
        dialog.Show();
    }




    #region login
    public TMP_InputField inputUsername;
    public TMP_InputField inputPassword;
    public Button btnLogin;


    private async void onClickLogin()
    {
        string username = inputUsername.text;
        string password = inputPassword.text;
        if(!await TemplateService.LoadTemplatesAysnc())
        {
            UIManager.Instance.ShowOK("Vui lòng thử lại!");
            return;
        }
        await LoginService.SendLoginRequest(username,password);
    }
    #endregion


    public void EnableGameScreen()
    {
        this.GameScreenCanvas.SetActive(true);
    }

    public void EnableLoginScreen()
    {
        UIManager.CurrentScreenType = ScreenType.LoginScreen;
        this.LoginScreenCanvas.gameObject.SetActive(true);
    }
    public void DisableLoginScreen()
    {
        this.LoginScreenCanvas.gameObject.SetActive(false);
    }

    public void EnableLoadScreen()
    {
        UIManager.CurrentScreenType = ScreenType.LoadScreen;
        this.LoadScreenCanvas.gameObject.SetActive(true);
    }


    public void DisableLoadScreen()
    {
        this.LoadScreenCanvas.gameObject.SetActive(false);
    }
    public void DisableGameScreen()
    {
        this.GameScreenCanvas.SetActive(false);
    }
}
