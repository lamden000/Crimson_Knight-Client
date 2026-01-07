using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogOK : DialogBase
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button okButton;

    private Action onOK;

    public void Setup(string message, Action onOkClicked = null)
    {
        if (messageText != null)
            messageText.text = message;

        onOK = onOkClicked;

        okButton.onClick.RemoveAllListeners();
        okButton.onClick.AddListener(() =>
        {
            onOK?.Invoke();  
            Close();         
        });
    }
}
