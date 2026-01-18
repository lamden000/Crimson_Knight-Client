using Assets.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RegisterScreenUIManager : BaseUIManager
{
    [SerializeField]
    private TMP_InputField inputUsername;
    [SerializeField]
    private TMP_InputField inputPassword;
    [SerializeField]
    private Button btnClass;
    [SerializeField]
    private Button btnGender;
    [SerializeField]
    private Button btnRegister;
    [SerializeField]
    private Button btnBackToLogin;
    [SerializeField]
    private Image previewImage;

    private ClassType currentClass = ClassType.CHIEN_BINH;
    private Gender currentGender = Gender.Male;

    // Tên hiển thị cho các class
    private readonly string[] classNames = { "Chiến Binh", "Sát Thủ", "Pháp Sư", "Xạ Thủ" };
    
    // Tên hiển thị cho gender
    private readonly string[] genderNames = { "Nam", "Nữ" };

    // Tên file cho các class (bỏ dấu, bỏ cách) - dùng cho load sprite
    private readonly string[] classFileNames = { "ChienBinh", "SatThu", "PhapSu", "XaThu" };
    
    // Tên file cho gender (bỏ dấu) - dùng cho load sprite
    private readonly string[] genderFileNames = { "Nam", "Nu" };

    void Awake()
    {
        // Setup button listeners
        btnClass.onClick.RemoveAllListeners();
        btnClass.onClick.AddListener(OnClickClass);

        btnGender.onClick.RemoveAllListeners();
        btnGender.onClick.AddListener(OnClickGender);

        btnRegister.onClick.RemoveAllListeners();
        btnRegister.onClick.AddListener(OnClickRegister);

        if (btnBackToLogin != null)
        {
            btnBackToLogin.onClick.RemoveAllListeners();
            btnBackToLogin.onClick.AddListener(OnClickBackToLogin);
        }

        // Initialize UI
        UpdateClassButtonText();
        UpdateGenderButtonText();
        UpdatePreviewImage();
    }

    private void OnClickClass()
    {
        // Chuyển sang class tiếp theo (luân phiên 4 class)
        int currentIndex = (int)currentClass;
        currentIndex = (currentIndex + 1) % 4; // 0, 1, 2, 3 rồi quay về 0
        currentClass = (ClassType)currentIndex;

        UpdateClassButtonText();
        UpdatePreviewImage();
        
        Debug.Log($"[RegisterScreen] Class changed to: {classNames[(int)currentClass]}");
    }

    private void OnClickGender()
    {
        // Toggle giữa Nam (0) và Nữ (1)
        if (currentGender == Gender.Male)
        {
            currentGender = Gender.Female;
        }
        else
        {
            currentGender = Gender.Male;
        }

        UpdateGenderButtonText();
        UpdatePreviewImage();
        
        Debug.Log($"[RegisterScreen] Gender changed to: {genderNames[(int)currentGender]}");
    }

    private void OnClickRegister()
    {
        string username = inputUsername.text;
        string password = inputPassword.text;

        // Validate input
        if (string.IsNullOrEmpty(username))
        {
            Debug.LogWarning("[RegisterScreen] Username is empty!");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("[RegisterScreen] Password is empty!");
            return;
        }

        // Debug ra lựa chọn
        Debug.Log("=== ĐĂNG KÝ ===");
        Debug.Log($"Tên tài khoản: {username}");
        Debug.Log($"Mật khẩu: {password}");
        Debug.Log($"Class: {classNames[(int)currentClass]} ({(int)currentClass})");
        Debug.Log($"Giới tính: {genderNames[(int)currentGender]} ({(int)currentGender})");
        Debug.Log("===============");

        // TODO: Gửi request đăng ký lên server
    }

    private void OnClickBackToLogin()
    {
        Debug.Log("[RegisterScreen] Back to Login button clicked - Switch to Login Screen");
        UIManager.Instance.EnableLoginScreen();
    }

    private void UpdateClassButtonText()
    {
        TextMeshProUGUI textComponent = btnClass.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = classNames[(int)currentClass];
        }
    }

    private void UpdateGenderButtonText()
    {
        TextMeshProUGUI textComponent = btnGender.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = genderNames[(int)currentGender];
        }
    }

    private void UpdatePreviewImage()
    {
        // Tạo path để load sprite preview
        // Format: "UI/CharacterPreviews/{className}_{genderName}"
        // Ví dụ: "UI/CharacterPreviews/XaThu_Nam", "UI/CharacterPreviews/ChienBinh_Nu"
        string className = classFileNames[(int)currentClass];
        string genderName = genderFileNames[(int)currentGender];
        string spritePath = $"UI/CharacterPreviews/{className}_{genderName}";
        
        // Load sprite
        Sprite previewSprite = Resources.Load<Sprite>(spritePath);
        
        if (previewSprite != null)
        {
            previewImage.sprite = previewSprite;
            previewImage.enabled = true;
            previewImage.color = Color.white;
            Debug.Log($"[RegisterScreen] Loaded preview sprite: {spritePath}");
        }
        else
        {
            // Nếu không tìm thấy, disable image
            previewImage.enabled = false;
            Debug.LogWarning($"[RegisterScreen] Preview sprite not found at: {spritePath}");
        }
    }
}
