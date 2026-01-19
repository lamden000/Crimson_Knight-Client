using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SoundSettingsUIManager : BaseUIManager
{
    [SerializeField]
    private Slider sliderBackgroundMusic;
    [SerializeField]
    private Slider sliderNoiseAndEffects;
    [SerializeField]
    private TextMeshProUGUI txtBackgroundMusicValue;
    [SerializeField]
    private TextMeshProUGUI txtNoiseAndEffectsValue;

    private const string PREFS_BACKGROUND_MUSIC_VOLUME = "BackgroundMusicVolume";
    private const string PREFS_NOISE_AND_EFFECTS_VOLUME = "NoiseAndEffectsVolume";

    void Awake()
    {
        // Setup slider listeners
        if (sliderBackgroundMusic != null)
        {
            sliderBackgroundMusic.onValueChanged.RemoveAllListeners();
            sliderBackgroundMusic.onValueChanged.AddListener(OnBackgroundMusicVolumeChanged);
        }

        if (sliderNoiseAndEffects != null)
        {
            sliderNoiseAndEffects.onValueChanged.RemoveAllListeners();
            sliderNoiseAndEffects.onValueChanged.AddListener(OnNoiseAndEffectsVolumeChanged);
        }

        // Load saved settings
        LoadSettings();
    }

    public override void ShowUI()
    {
        base.ShowUI();
        // Reload settings khi mở panel để đảm bảo hiển thị đúng giá trị
        LoadSettings();
    }

    private void OnBackgroundMusicVolumeChanged(float value)
    {
        // Cập nhật volume cho background music (map music)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBackgroundMusicVolume(value);
        }

        // Cập nhật text hiển thị
        if (txtBackgroundMusicValue != null)
        {
            txtBackgroundMusicValue.text = Mathf.RoundToInt(value * 100).ToString() + "%";
        }

        // Lưu vào PlayerPrefs
        PlayerPrefs.SetFloat(PREFS_BACKGROUND_MUSIC_VOLUME, value);
        PlayerPrefs.Save();

    }

    private void OnNoiseAndEffectsVolumeChanged(float value)
    {
        // Cập nhật volume cho noise và effects (map noise + attack effects)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetNoiseAndEffectsVolume(value);
        }

        // Cập nhật text hiển thị
        if (txtNoiseAndEffectsValue != null)
        {
            txtNoiseAndEffectsValue.text = Mathf.RoundToInt(value * 100).ToString() + "%";
        }

        // Lưu vào PlayerPrefs
        PlayerPrefs.SetFloat(PREFS_NOISE_AND_EFFECTS_VOLUME, value);
        PlayerPrefs.Save();

    }

    private void LoadSettings()
    {
        // Load background music volume (mặc định 1.0 nếu chưa có)
        float bgMusicVolume = PlayerPrefs.GetFloat(PREFS_BACKGROUND_MUSIC_VOLUME, 1.0f);
        if (sliderBackgroundMusic != null)
        {
            sliderBackgroundMusic.value = bgMusicVolume;
        }
        if (txtBackgroundMusicValue != null)
        {
            txtBackgroundMusicValue.text = Mathf.RoundToInt(bgMusicVolume * 100).ToString() + "%";
        }

        // Load noise and effects volume (mặc định 1.0 nếu chưa có)
        float noiseAndEffectsVolume = PlayerPrefs.GetFloat(PREFS_NOISE_AND_EFFECTS_VOLUME, 1.0f);
        if (sliderNoiseAndEffects != null)
        {
            sliderNoiseAndEffects.value = noiseAndEffectsVolume;
        }
        if (txtNoiseAndEffectsValue != null)
        {
            txtNoiseAndEffectsValue.text = Mathf.RoundToInt(noiseAndEffectsVolume * 100).ToString() + "%";
        }

        // Áp dụng settings vào AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBackgroundMusicVolume(bgMusicVolume);
            AudioManager.Instance.SetNoiseAndEffectsVolume(noiseAndEffectsVolume);
        }

    }
}
