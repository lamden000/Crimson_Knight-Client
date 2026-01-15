using Assets.Scripts.Networking;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogYesNo : DialogBase
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private Action<bool, int> callback; 
    private int dialogId;            

    public void Setup(string message, int id, Action<bool, int> onResult)
    {
        dialogId = id;
        callback = onResult;

        if (messageText != null)
            messageText.text = message;

        yesButton.onClick.RemoveAllListeners();
        yesButton.onClick.AddListener(() =>
        {
            RequestManager.SelectDialogYesNo(DialogYesNoId.ENTER_PHO_BAN, true);
            Debug.Log($"Yes clicked on dialog id = {dialogId}");
            callback?.Invoke(true, dialogId);
            Close();
        });

        noButton.onClick.RemoveAllListeners();
        noButton.onClick.AddListener(() =>
        {
            RequestManager.SelectDialogYesNo(DialogYesNoId.ENTER_PHO_BAN, false);
            Debug.Log($"No clicked on dialog id = {dialogId}");
            callback?.Invoke(false, dialogId);
            Close();
        });
    }
}
