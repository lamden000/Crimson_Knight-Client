using Assets.Scripts.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginScreenUIManager : BaseUIManager
{
    [SerializeField]
    private TMP_InputField inputUsername;
    [SerializeField]
    private TMP_InputField inputPassword;
    [SerializeField]
    private Button btnLogin;
    [SerializeField]
    private Button btnRegister;


    void Awake()
    {
        btnLogin.onClick.RemoveAllListeners();
        btnLogin.onClick.AddListener(onClickLogin);

        if (btnRegister != null)
        {
            btnRegister.onClick.RemoveAllListeners();
            btnRegister.onClick.AddListener(onClickRegister);
        }
    }

    private async void onClickLogin()
    {
        Debug.Log("[UIManager] Login button clicked");

        string username = inputUsername.text;
        string password = inputPassword.text;

        UIManager.Instance.EnableLoadScreen();

        if (!await TemplateService.LoadTemplatesAysnc())
        {
            Debug.LogError("[UIManager] LoadTemplatesAysnc failed");
            UIManager.Instance.ShowOK("Vui lòng thử lại!");
            UIManager.Instance.EnableLoginScreen();
            return;
        }

        Debug.Log("[UIManager] SendLoginRequest");
        await LoginService.SendLoginRequest(username, password);
    }

    private void onClickRegister()
    {
        Debug.Log("[UIManager] Register button clicked - Switch to Register Screen");
        UIManager.Instance.EnableRegisterScreen();
    }
}
