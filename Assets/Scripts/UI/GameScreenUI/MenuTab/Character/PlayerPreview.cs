using UnityEngine;
using Assets.Scripts.GameManager.Players;

public class PlayerPreview : MonoBehaviour
{
    public Camera previewCamera;
    public Transform previewRoot;

    [Header("Camera Settings")]
    public float characterPreviewSize = 5f; // Size camera cho preview character
    public float skillPreviewSize = 10f;   // Size camera cho preview skill

    [Header("Dummy Player")]
    public GameObject dummyPlayer;

    private PlayerAnimationController dummyAnimController;
    private Character dummyCharacter;

    private void Awake()
    {
        if (dummyPlayer == null)
        {
            Debug.LogError("[PlayerPreview] Dummy player GameObject chưa được gán!");
            return;
        }

        dummyAnimController = dummyPlayer.GetComponent<PlayerAnimationController>();
        dummyCharacter = dummyPlayer.GetComponent<Character>();

        if (dummyAnimController == null)
        {
            Debug.LogError("[PlayerPreview] Dummy player thiếu PlayerAnimationController component!");
        }

        if (dummyCharacter == null)
        {
            Debug.LogError("[PlayerPreview] Dummy player thiếu Character component!");
        }

        // Disable toàn bộ logic của dummy (trừ PlayerAnimationController và Character)
        foreach (var mb in dummyPlayer.GetComponentsInChildren<MonoBehaviour>())
        {
            if (mb != dummyAnimController && mb != dummyCharacter && mb != this)
            {
                mb.enabled = false;
            }
        }

        // Set layer cho camera nhìn thấy
        int layer = LayerMask.NameToLayer("PlayerPreview");
        SetLayerRecursively(dummyPlayer, layer);
    }

    public void Show(Player sourcePlayer)
    {
        if (dummyPlayer == null || dummyAnimController == null || dummyCharacter == null)
        {
            Debug.LogError("[PlayerPreview] Dummy player chưa được setup đúng!");
            return;
        }

        if (sourcePlayer == null)
        {
            Debug.LogWarning("[PlayerPreview] Source player is null!");
            return;
        }

        // Set camera size cho character preview
        if (previewCamera != null)
        {
            previewCamera.orthographicSize = characterPreviewSize;
        }

        // Setup dummy giống player
        SetupDummy(sourcePlayer);
    }

    public void SetCameraSizeForSkill()
    {
        if (previewCamera != null)
        {
            previewCamera.orthographicSize = skillPreviewSize;
        }
    }

    public void SetCameraSizeForCharacter()
    {
        if (previewCamera != null)
        {
            previewCamera.orthographicSize = characterPreviewSize;
        }
    }

    private void SetupDummy(Player sourcePlayer)
    {
        // Kiểm tra CharacterSpriteDatabase.Instance
        if (CharacterSpriteDatabase.Instance == null)
        {
            Debug.LogError("[PlayerPreview] CharacterSpriteDatabase.Instance is null! Cần đảm bảo CharacterSpriteDatabase đã được khởi tạo trong scene.");
            return;
        }

        // Đảm bảo Character component tồn tại
        if (dummyCharacter == null)
        {
            dummyCharacter = dummyPlayer.GetComponent<Character>();
            if (dummyCharacter == null)
            {
                Debug.LogError("[PlayerPreview] Không tìm thấy Character component trên dummy player!");
                return;
            }
        }

        // Đảm bảo dummy player GameObject active để Awake() chạy và khởi tạo dictionaries
        if (!dummyPlayer.activeInHierarchy)
        {
            dummyPlayer.SetActive(true);
        }

        // Đảm bảo PlayerAnimationController được enable
        if (!dummyAnimController.enabled)
        {
            dummyAnimController.enabled = true;
        }

        // Đảm bảo Character component được enable
        if (!dummyCharacter.enabled)
        {
            dummyCharacter.enabled = true;
        }

        // Setup Character component trước (cần thiết cho SetUp của PlayerAnimationController)
        dummyCharacter.SetUp(sourcePlayer.ClassType);

        // Setup PlayerAnimationController (sau khi Character đã được setup)
        // SetUp() sẽ gọi GetComponent<Character>() và cần character đã được setup
        dummyAnimController.SetUp();

        // Set animation state để hiển thị idle
        dummyAnimController.SetAnimation(Direction.Down, State.Idle);

        // Load base parts (hair, body, legs) dựa trên Gender
        LoadBaseParts(sourcePlayer.Gender);

        // Load equipment đang mặc
        LoadEquipment(sourcePlayer);
    }

    private void LoadBaseParts(Gender gender)
    {
        if (dummyAnimController == null) return;

        if (gender == Gender.Male)
        {
            dummyAnimController.LoadPart(CharacterPart.Hair, 0);
            dummyAnimController.LoadPart(CharacterPart.Body, 0);
            dummyAnimController.LoadPart(CharacterPart.Legs, 0);
        }
        else
        {
            dummyAnimController.LoadPart(CharacterPart.Hair, 41);
            dummyAnimController.LoadPart(CharacterPart.Body, 3);
            dummyAnimController.LoadPart(CharacterPart.Legs, 3);
        }
    }

    private void LoadEquipment(Player sourcePlayer)
    {
        if (dummyAnimController == null || sourcePlayer == null) return;

        // Load weapon
        ItemEquipment weapon = sourcePlayer.GetVuKhi();
        int weaponPartId = -1;
        if (weapon != null)
        {
            weaponPartId = TemplateManager.ItemEquipmentTemplates[weapon.TemplateId].PartId;
        }
        dummyAnimController.LoadPartVuKhi(sourcePlayer.ClassType, weaponPartId);

        // Load armor (body)
        ItemEquipment armor = sourcePlayer.GetAo();
        int armorPartId = -1;
        if (armor == null)
        {
            if (sourcePlayer.Gender == Gender.Male)
            {
                armorPartId = 0;
            }
            else
            {
                armorPartId = 3;
            }
        }
        else
        {
            armorPartId = TemplateManager.ItemEquipmentTemplates[armor.TemplateId].PartId;
        }
        dummyAnimController.LoadPart(CharacterPart.Body, armorPartId);

        // Load pants (legs)
        ItemEquipment pants = sourcePlayer.GetQuan();
        int pantsPartId = -1;
        if (pants == null)
        {
            if (sourcePlayer.Gender == Gender.Male)
            {
                pantsPartId = 0;
            }
            else
            {
                pantsPartId = 3;
            }
        }
        else
        {
            pantsPartId = TemplateManager.ItemEquipmentTemplates[pants.TemplateId].PartId;
        }
        dummyAnimController.LoadPart(CharacterPart.Legs, pantsPartId);
    }

    public void Clear()
    {
        // Không cần destroy dummy vì nó là prefab có sẵn trong scene
        // Chỉ cần reset về trạng thái ban đầu nếu cần
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}
