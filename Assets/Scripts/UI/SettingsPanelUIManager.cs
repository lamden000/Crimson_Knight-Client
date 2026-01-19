using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelUIManager : BaseUIManager
{
    [SerializeField]
    private Button btnSoundSetting;
    [SerializeField]
    private Button btnClose;
    [SerializeField]
    private SoundSettingsUIManager soundSettingsUIManager;

    void Awake()
    {
        if (btnSoundSetting != null)
        {
            btnSoundSetting.onClick.RemoveAllListeners();
            btnSoundSetting.onClick.AddListener(OnClickSoundSetting);
        }

        if (btnClose != null)
        {
            btnClose.onClick.RemoveAllListeners();
            btnClose.onClick.AddListener(OnClickClose);
        }
    }

    private void OnClickSoundSetting()
    {
        Debug.Log("[SettingsPanel] Sound Setting button clicked");
        if (soundSettingsUIManager != null)
        {
            soundSettingsUIManager.ShowUI();
        }
    }

    private void OnClickClose()
    {
        Debug.Log("[SettingsPanel] Close button clicked");
        HideUI();
        // Đóng cả sound settings panel khi đóng settings panel
        if (soundSettingsUIManager != null)
        {
            soundSettingsUIManager.HideUI();
        }
    }

    public override void ShowUI()
    {
        base.ShowUI();
        // Không ẩn sound settings panel, để chúng có thể hiển thị song song
    }
}
