using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TalkingUIManager : BaseUIManager
{
    [SerializeField]
    private TMP_Text txtTitle;
    [SerializeField]
    private TMP_Text txtContent;
    [SerializeField]
    private Button btnYes;
    [SerializeField]
    private Button btnNo;

    private Action actionYes;
    private Action actionNo;

    void Awake()
    {
        btnYes.onClick.RemoveAllListeners();
        btnYes.onClick.AddListener(onClickYes);
        btnNo.onClick.RemoveAllListeners();
        btnNo.onClick.AddListener(onClickNo);
    }

    private void onClickNo()
    {
        actionNo?.Invoke();
    }

    private void onClickYes()
    {
        actionYes?.Invoke();
    }

    public void Setup(string title, string content, Action onYesClicked = null, Action onNoCliked = null)
    {
        txtTitle.text = title;
        txtContent.text = content;
        actionYes = onYesClicked;
        actionNo = onNoCliked;
    }
}
