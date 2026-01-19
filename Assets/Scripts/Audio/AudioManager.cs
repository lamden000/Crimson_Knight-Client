using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapMusicEntry
{
    public int mapId;
    [Tooltip("Background music cho map này. Nếu null thì không phát.")]
    public AudioClip musicClip;
    [Tooltip("Ambient noise/audio cho map này (tiếng gà, chim, quái vật, gió...). Nếu null thì không phát.")]
    public AudioClip noiseClip;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music Sources")]
    [Tooltip("AudioSource cho nhạc nền login và game")]
    public AudioSource musicSource;
    
    [Tooltip("AudioSource riêng cho nhạc nền map")]
    public AudioSource mapMusicSource;
    
    [Tooltip("AudioSource riêng cho ambient noise/audio của map (tiếng gà, chim, quái vật, gió...)")]
    public AudioSource mapNoiseSource;
    
    [Tooltip("AudioSource riêng cho các effect âm thanh tấn công và hiệu ứng")]
    public AudioSource attackEffectSource;

    [Header("Login Music")]
    public AudioClip loginMusic;

    [Header("Attack Sounds")]
    [Tooltip("Âm thanh tấn công cho Chiến Binh")]
    public AudioClip attackSoundChienBinh;
    
    [Tooltip("Âm thanh tấn công cho Sát Thủ")]
    public AudioClip attackSoundSatThu;
    
    [Tooltip("Âm thanh tấn công cho Pháp Sư")]
    public AudioClip attackSoundPhapSu;
    
    [Tooltip("Âm thanh tấn công cho Xạ Thủ")]
    public AudioClip attackSoundXaThu;

    [Header("Map Audio Settings")]
    [Tooltip("Danh sách map ID với background music và noise audio. Mỗi map có thể có music và/hoặc noise riêng. Nếu null thì không phát.")]
    public List<MapMusicEntry> mapMusicList = new List<MapMusicEntry>();

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [Tooltip("Volume cho nhạc nền login")]
    public float loginMusicVolume = 1f;

    [Range(0f, 1f)]
    [Tooltip("Volume cho background music của map")]
    public float mapMusicVolume = 1f;

    [Range(0f, 1f)]
    [Tooltip("Volume cho ambient noise/audio của map")]
    public float mapNoiseVolume = 1f;

    [Range(0f, 1f)]
    [Tooltip("Volume cho các effect âm thanh tấn công")]
    public float attackEffectVolume = 1f;

    private Dictionary<int, MapMusicEntry> mapAudioDictionary;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Build dictionary từ list để tìm kiếm nhanh hơn
        BuildMapMusicDictionary();

        // Load saved settings
        LoadSettings();
    }

    private void BuildMapMusicDictionary()
    {
        mapAudioDictionary = new Dictionary<int, MapMusicEntry>();
        foreach (var entry in mapMusicList)
        {
            if (mapAudioDictionary.ContainsKey(entry.mapId))
            {
                Debug.LogWarning($"[AudioManager] Duplicate map ID {entry.mapId} found. Using the first entry.");
            }
            else
            {
                mapAudioDictionary[entry.mapId] = entry;
            }
        }
    }

    /// <summary>
    /// Phát nhạc nền cho màn hình đăng nhập
    /// </summary>
    public void PlayLoginMusic()
    {
        if (loginMusic == null)
        {
            Debug.LogWarning("[AudioManager] Login music is not assigned!");
            return;
        }

        // Load volume từ settings (background music volume)
        float bgMusicVolume = PlayerPrefs.GetFloat("BackgroundMusicVolume", 1.0f);
        musicSource.volume = bgMusicVolume;
        PlayMusic(loginMusic);
    }

    /// <summary>
    /// Phát background music và noise audio cho map theo Map ID. Nếu null thì không phát.
    /// </summary>
    /// <param name="mapId">ID của map</param>
    public void PlayMapMusic(int mapId)
    {
        // Dừng nhạc login nếu đang phát
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }

        // Dừng map music và noise cũ nếu đang phát
        StopMapAudio();

        if (mapAudioDictionary == null || mapAudioDictionary.Count == 0)
        {
            Debug.LogWarning($"[AudioManager] Map audio dictionary is empty. No audio will play for Map ID: {mapId}");
            return;
        }

        if (mapAudioDictionary.TryGetValue(mapId, out MapMusicEntry entry))
        {
            // Phát background music nếu có
            if (entry.musicClip != null)
            {
                if (mapMusicSource == null)
                {
                    Debug.LogError("[AudioManager] Map music source is not assigned!");
                }
                else
                {
                    mapMusicSource.volume = mapMusicVolume;
                    PlayMapMusicClip(entry.musicClip);
                }
            }
            else
            {
                Debug.Log($"[AudioManager] No background music for Map ID: {mapId}");
            }

            // Phát noise/ambient audio nếu có
            if (entry.noiseClip != null)
            {
                if (mapNoiseSource == null)
                {
                    Debug.LogError("[AudioManager] Map noise source is not assigned!");
                }
                else
                {
                    mapNoiseSource.volume = mapNoiseVolume;
                    PlayMapNoiseClip(entry.noiseClip);
                }
            }
            else
            {
                Debug.Log($"[AudioManager] No noise/ambient audio for Map ID: {mapId}");
            }
        }
        else
        {
            Debug.LogWarning($"[AudioManager] No audio entry found for Map ID: {mapId}. No audio will play.");
        }
    }

    /// <summary>
    /// Kiểm tra xem có audio entry cho map ID này không
    /// </summary>
    /// <param name="mapId">ID của map</param>
    /// <returns>True nếu có entry cho map này</returns>
    public bool HasMapAudio(int mapId)
    {
        return mapAudioDictionary != null && mapAudioDictionary.ContainsKey(mapId);
    }

    /// <summary>
    /// Kiểm tra xem có background music cho map ID này không
    /// </summary>
    /// <param name="mapId">ID của map</param>
    /// <returns>True nếu có background music cho map này</returns>
    public bool HasMapMusic(int mapId)
    {
        if (mapAudioDictionary != null && mapAudioDictionary.TryGetValue(mapId, out MapMusicEntry entry))
        {
            return entry.musicClip != null;
        }
        return false;
    }

    /// <summary>
    /// Kiểm tra xem có noise audio cho map ID này không
    /// </summary>
    /// <param name="mapId">ID của map</param>
    /// <returns>True nếu có noise audio cho map này</returns>
    public bool HasMapNoise(int mapId)
    {
        if (mapAudioDictionary != null && mapAudioDictionary.TryGetValue(mapId, out MapMusicEntry entry))
        {
            return entry.noiseClip != null;
        }
        return false;
    }

    /// <summary>
    /// Lấy MapMusicEntry cho map ID cụ thể
    /// </summary>
    /// <param name="mapId">ID của map</param>
    /// <returns>MapMusicEntry hoặc null nếu không tìm thấy</returns>
    public MapMusicEntry GetMapAudio(int mapId)
    {
        if (mapAudioDictionary != null && mapAudioDictionary.TryGetValue(mapId, out MapMusicEntry entry))
        {
            return entry;
        }
        return null;
    }

    /// <summary>
    /// Lấy background music cho map ID cụ thể
    /// </summary>
    /// <param name="mapId">ID của map</param>
    /// <returns>AudioClip hoặc null nếu không tìm thấy</returns>
    public AudioClip GetMapMusic(int mapId)
    {
        if (mapAudioDictionary != null && mapAudioDictionary.TryGetValue(mapId, out MapMusicEntry entry))
        {
            return entry.musicClip;
        }
        return null;
    }

    /// <summary>
    /// Lấy noise audio cho map ID cụ thể
    /// </summary>
    /// <param name="mapId">ID của map</param>
    /// <returns>AudioClip hoặc null nếu không tìm thấy</returns>
    public AudioClip GetMapNoise(int mapId)
    {
        if (mapAudioDictionary != null && mapAudioDictionary.TryGetValue(mapId, out MapMusicEntry entry))
        {
            return entry.noiseClip;
        }
        return null;
    }

    /// <summary>
    /// Dừng tất cả nhạc nền (login và map audio)
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
        if (mapMusicSource != null && mapMusicSource.isPlaying)
        {
            mapMusicSource.Stop();
        }
        if (mapNoiseSource != null && mapNoiseSource.isPlaying)
        {
            mapNoiseSource.Stop();
        }
        Debug.Log("[AudioManager] All music stopped");
    }

    /// <summary>
    /// Dừng nhạc nền login/game
    /// </summary>
    public void StopLoginGameMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
            Debug.Log("[AudioManager] Login/Game music stopped");
        }
    }

    /// <summary>
    /// Dừng tất cả audio của map (background music và noise)
    /// </summary>
    public void StopMapAudio()
    {
        if (mapMusicSource != null && mapMusicSource.isPlaying)
        {
            mapMusicSource.Stop();
        }
        if (mapNoiseSource != null && mapNoiseSource.isPlaying)
        {
            mapNoiseSource.Stop();
        }
        Debug.Log("[AudioManager] Map audio stopped");
    }

    /// <summary>
    /// Dừng background music của map
    /// </summary>
    public void StopMapMusic()
    {
        if (mapMusicSource != null && mapMusicSource.isPlaying)
        {
            mapMusicSource.Stop();
            Debug.Log("[AudioManager] Map music stopped");
        }
    }

    /// <summary>
    /// Dừng noise/ambient audio của map
    /// </summary>
    public void StopMapNoise()
    {
        if (mapNoiseSource != null && mapNoiseSource.isPlaying)
        {
            mapNoiseSource.Stop();
            Debug.Log("[AudioManager] Map noise stopped");
        }
    }

    /// <summary>
    /// Tạm dừng tất cả nhạc nền
    /// </summary>
    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
        }
        if (mapMusicSource != null && mapMusicSource.isPlaying)
        {
            mapMusicSource.Pause();
        }
        if (mapNoiseSource != null && mapNoiseSource.isPlaying)
        {
            mapNoiseSource.Pause();
        }
        Debug.Log("[AudioManager] All music paused");
    }

    /// <summary>
    /// Tạm dừng nhạc nền login/game
    /// </summary>
    public void PauseLoginGameMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
            Debug.Log("[AudioManager] Login/Game music paused");
        }
    }

    /// <summary>
    /// Tạm dừng tất cả audio của map
    /// </summary>
    public void PauseMapAudio()
    {
        if (mapMusicSource != null && mapMusicSource.isPlaying)
        {
            mapMusicSource.Pause();
        }
        if (mapNoiseSource != null && mapNoiseSource.isPlaying)
        {
            mapNoiseSource.Pause();
        }
        Debug.Log("[AudioManager] Map audio paused");
    }

    /// <summary>
    /// Tạm dừng background music của map
    /// </summary>
    public void PauseMapMusic()
    {
        if (mapMusicSource != null && mapMusicSource.isPlaying)
        {
            mapMusicSource.Pause();
            Debug.Log("[AudioManager] Map music paused");
        }
    }

    /// <summary>
    /// Tạm dừng noise/ambient audio của map
    /// </summary>
    public void PauseMapNoise()
    {
        if (mapNoiseSource != null && mapNoiseSource.isPlaying)
        {
            mapNoiseSource.Pause();
            Debug.Log("[AudioManager] Map noise paused");
        }
    }

    /// <summary>
    /// Tiếp tục phát tất cả nhạc nền
    /// </summary>
    public void ResumeMusic()
    {
        if (musicSource != null && !musicSource.isPlaying && musicSource.clip != null)
        {
            musicSource.Play();
        }
        if (mapMusicSource != null && !mapMusicSource.isPlaying && mapMusicSource.clip != null)
        {
            mapMusicSource.Play();
        }
        if (mapNoiseSource != null && !mapNoiseSource.isPlaying && mapNoiseSource.clip != null)
        {
            mapNoiseSource.Play();
        }
        Debug.Log("[AudioManager] All music resumed");
    }

    /// <summary>
    /// Tiếp tục phát nhạc nền login/game
    /// </summary>
    public void ResumeLoginGameMusic()
    {
        if (musicSource != null && !musicSource.isPlaying && musicSource.clip != null)
        {
            musicSource.Play();
            Debug.Log("[AudioManager] Login/Game music resumed");
        }
    }

    /// <summary>
    /// Tiếp tục phát tất cả audio của map
    /// </summary>
    public void ResumeMapAudio()
    {
        if (mapMusicSource != null && !mapMusicSource.isPlaying && mapMusicSource.clip != null)
        {
            mapMusicSource.Play();
        }
        if (mapNoiseSource != null && !mapNoiseSource.isPlaying && mapNoiseSource.clip != null)
        {
            mapNoiseSource.Play();
        }
        Debug.Log("[AudioManager] Map audio resumed");
    }

    /// <summary>
    /// Tiếp tục phát background music của map
    /// </summary>
    public void ResumeMapMusic()
    {
        if (mapMusicSource != null && !mapMusicSource.isPlaying && mapMusicSource.clip != null)
        {
            mapMusicSource.Play();
            Debug.Log("[AudioManager] Map music resumed");
        }
    }

    /// <summary>
    /// Tiếp tục phát noise/ambient audio của map
    /// </summary>
    public void ResumeMapNoise()
    {
        if (mapNoiseSource != null && !mapNoiseSource.isPlaying && mapNoiseSource.clip != null)
        {
            mapNoiseSource.Play();
            Debug.Log("[AudioManager] Map noise resumed");
        }
    }

    /// <summary>
    /// Đặt volume cho nhạc nền login/game
    /// </summary>
    /// <param name="volume">Volume từ 0 đến 1</param>
    public void SetLoginGameMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }
    }

    /// <summary>
    /// Đặt volume cho background music của map
    /// </summary>
    /// <param name="volume">Volume từ 0 đến 1</param>
    public void SetMapMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (mapMusicSource != null)
        {
            mapMusicSource.volume = volume;
        }
        mapMusicVolume = volume;
    }

    /// <summary>
    /// Đặt volume cho noise/ambient audio của map
    /// </summary>
    /// <param name="volume">Volume từ 0 đến 1</param>
    public void SetMapNoiseVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (mapNoiseSource != null)
        {
            mapNoiseSource.volume = volume;
        }
        mapNoiseVolume = volume;
    }

    /// <summary>
    /// Đặt volume cho tất cả nhạc nền
    /// </summary>
    /// <param name="volume">Volume từ 0 đến 1</param>
    public void SetMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }
        if (mapMusicSource != null)
        {
            mapMusicSource.volume = volume;
        }
        if (mapNoiseSource != null)
        {
            mapNoiseSource.volume = volume;
        }
    }

    /// <summary>
    /// Đặt volume cho background music (map music + login music) - dùng cho settings
    /// </summary>
    /// <param name="volume">Volume từ 0 đến 1</param>
    public void SetBackgroundMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        // Áp dụng cho cả map music và login music
        SetMapMusicVolume(volume);
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }
    }

    /// <summary>
    /// Đặt volume cho noise và effects (map noise + attack effects) - dùng cho settings
    /// </summary>
    /// <param name="volume">Volume từ 0 đến 1</param>
    public void SetNoiseAndEffectsVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        SetMapNoiseVolume(volume);
        SetAttackEffectVolume(volume);
    }

    /// <summary>
    /// Load settings từ PlayerPrefs
    /// </summary>
    public void LoadSettings()
    {
        const string PREFS_BACKGROUND_MUSIC_VOLUME = "BackgroundMusicVolume";
        const string PREFS_NOISE_AND_EFFECTS_VOLUME = "NoiseAndEffectsVolume";

        float bgMusicVolume = PlayerPrefs.GetFloat(PREFS_BACKGROUND_MUSIC_VOLUME, 1.0f);
        float noiseAndEffectsVolume = PlayerPrefs.GetFloat(PREFS_NOISE_AND_EFFECTS_VOLUME, 1.0f);

        SetBackgroundMusicVolume(bgMusicVolume);
        SetNoiseAndEffectsVolume(noiseAndEffectsVolume);

    }

    /// <summary>
    /// Phát nhạc trên AudioSource chính (login/game music)
    /// </summary>
    private void PlayMusic(AudioClip clip)
    {
        if (musicSource == null)
        {
            Debug.LogError("[AudioManager] Music source is not assigned!");
            return;
        }

        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] AudioClip is null!");
            return;
        }

        // Nếu đang phát cùng clip thì không cần phát lại
        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            return;
        }

        // Dừng map audio nếu đang phát
        StopMapAudio();

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    /// <summary>
    /// Phát background music trên AudioSource map music
    /// </summary>
    private void PlayMapMusicClip(AudioClip clip)
    {
        if (mapMusicSource == null)
        {
            Debug.LogError("[AudioManager] Map music source is not assigned!");
            return;
        }

        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] AudioClip is null!");
            return;
        }

        // Nếu đang phát cùng clip thì không cần phát lại
        if (mapMusicSource.clip == clip && mapMusicSource.isPlaying)
        {
            return;
        }

        mapMusicSource.Stop();
        mapMusicSource.clip = clip;
        mapMusicSource.loop = true;
        mapMusicSource.Play();
    }

    /// <summary>
    /// Phát noise/ambient audio trên AudioSource map noise
    /// </summary>
    private void PlayMapNoiseClip(AudioClip clip)
    {
        if (mapNoiseSource == null)
        {
            Debug.LogError("[AudioManager] Map noise source is not assigned!");
            return;
        }

        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] AudioClip is null!");
            return;
        }

        // Nếu đang phát cùng clip thì không cần phát lại
        if (mapNoiseSource.clip == clip && mapNoiseSource.isPlaying)
        {
            return;
        }

        mapNoiseSource.Stop();
        mapNoiseSource.clip = clip;
        mapNoiseSource.loop = true;
        mapNoiseSource.Play();
    }

    /// <summary>
    /// Phát âm thanh tấn công dựa trên class của nhân vật
    /// </summary>
    /// <param name="classType">Class của nhân vật (CHIEN_BINH, SAT_THU, PHAP_SU, XA_THU)</param>
    public void PlayAttackSound(ClassType classType)
    {
        if (attackEffectSource == null)
        {
            Debug.LogError("[AudioManager] Attack effect source is not assigned!");
            return;
        }

        AudioClip clipToPlay = null;

        switch (classType)
        {
            case ClassType.CHIEN_BINH:
                clipToPlay = attackSoundChienBinh;
                break;
            case ClassType.SAT_THU:
                clipToPlay = attackSoundSatThu;
                break;
            case ClassType.PHAP_SU:
                clipToPlay = attackSoundPhapSu;
                break;
            case ClassType.XA_THU:
                clipToPlay = attackSoundXaThu;
                break;
            case ClassType.NONE:
                // Nếu là NONE, không phát âm thanh
                Debug.Log("[AudioManager] ClassType is NONE, no attack sound will play.");
                return;
        }

        if (clipToPlay != null)
        {
            attackEffectSource.volume = attackEffectVolume;
            attackEffectSource.PlayOneShot(clipToPlay);
            Debug.Log($"[AudioManager] Playing attack sound for class: {classType}");
        }
        else
        {
            Debug.LogWarning($"[AudioManager] Attack sound not assigned for class: {classType}");
        }
    }

    /// <summary>
    /// Phát một shot âm thanh effect tấn công (dùng cho các effect khác sau này)
    /// </summary>
    /// <param name="clip">AudioClip cần phát</param>
    public void PlayAttackEffect(AudioClip clip)
    {
        if (attackEffectSource == null)
        {
            Debug.LogError("[AudioManager] Attack effect source is not assigned!");
            return;
        }

        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] AudioClip is null!");
            return;
        }

        attackEffectSource.volume = attackEffectVolume;
        attackEffectSource.PlayOneShot(clip);
    }

    /// <summary>
    /// Đặt volume cho attack effect source
    /// </summary>
    /// <param name="volume">Volume từ 0 đến 1</param>
    public void SetAttackEffectVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (attackEffectSource != null)
        {
            attackEffectSource.volume = volume;
        }
        attackEffectVolume = volume;
    }

    // Hàm này được gọi khi thay đổi trong Inspector (chỉ trong Editor)
    private void OnValidate()
    {
        // Rebuild dictionary khi thay đổi list trong Inspector
        if (Application.isPlaying && mapAudioDictionary != null)
        {
            BuildMapMusicDictionary();
        }
    }
}
