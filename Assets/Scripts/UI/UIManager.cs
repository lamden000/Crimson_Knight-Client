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

    public Canvas LoginScreenCanvas;
    public Canvas LoadScreenCanvas;
    public GameObject GameScreenCanvas;

    #region login
    public TMP_InputField inputUsername;
    public TMP_InputField inputPassword;
    public Button btnLogin;
    #endregion

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        btnLogin.onClick.RemoveAllListeners();
        btnLogin.onClick.AddListener(onClickLogin);

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

        this.LoginScreenCanvas.gameObject.SetActive(true);
        this.LoadScreenCanvas.gameObject.SetActive(false);
        this.GameScreenCanvas.SetActive(false);

        Debug.Log("[UIManager] EnableLoginScreen");
        Debug.Log("[UIManager] Play Login Music");

        AudioManager.Instance?.PlayLoginMusic();
    }

    public void DisableLoginScreen()
    {
        this.LoginScreenCanvas.gameObject.SetActive(false);
        Debug.Log("[UIManager] DisableLoginScreen");
    }

    public void EnableLoadScreen()
    {
        UIManager.CurrentScreenType = ScreenType.LoadScreen;

        this.LoginScreenCanvas.gameObject.SetActive(false);
        this.LoadScreenCanvas.gameObject.SetActive(true);
        this.GameScreenCanvas.SetActive(false);

        Debug.Log("[UIManager] EnableLoadScreen (keep current music)");
        // Không đổi nhạc vì AudioManager không có nhạc load
    }

    public void DisableLoadScreen()
    {
        this.LoadScreenCanvas.gameObject.SetActive(false);
        Debug.Log("[UIManager] DisableLoadScreen");
    }

    public void EnableGameScreen()
    {
        this.GameScreenCanvas.SetActive(true);
        this.LoginScreenCanvas.gameObject.SetActive(false);
        this.LoadScreenCanvas.gameObject.SetActive(false);

        Debug.Log("[UIManager] EnableGameScreen");
        Debug.Log("[UIManager] Play Game Music");

        AudioManager.Instance?.PlayGameMusic();
    }

    public void DisableGameScreen()
    {
        this.GameScreenCanvas.SetActive(false);
        Debug.Log("[UIManager] DisableGameScreen");
    }

    #endregion

    #region login logic

    private async void onClickLogin()
    {
        Debug.Log("[UIManager] Login button clicked");

        string username = inputUsername.text;
        string password = inputPassword.text;

        EnableLoadScreen();

        if (!await TemplateService.LoadTemplatesAysnc())
        {
            Debug.LogError("[UIManager] LoadTemplatesAysnc failed");
            UIManager.Instance.ShowOK("Vui lòng thử lại!");
            EnableLoginScreen();
            return;
        }

        Debug.Log("[UIManager] SendLoginRequest");
        await LoginService.SendLoginRequest(username, password);
    }

    #endregion
}
