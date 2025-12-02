using Assets.Scripts;
using Assets.Scripts.Map;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static ScreenType CurrentScreenType = ScreenType.LoginScreen;


    public static UIManager Instance { get; private set; }

    [SerializeField] private DialogFactory dialogFactory;

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


    private void onClickLogin()
    {
        string username = inputUsername.text;
        string password = inputPassword.text;
        Debug.Log($"username: {username}, password: {password}");
        MapManager.LoadMapById(int.Parse(username));
    }
    #endregion
}
