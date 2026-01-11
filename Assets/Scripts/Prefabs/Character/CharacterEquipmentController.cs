using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.GameManager.Players;

public class CharacterEquipmentController : MonoBehaviour
{
    [Header("References")]
    public TMP_InputField outfitInput;
    public TMP_InputField hairInput;
    public TMP_InputField headInput;
    public TMP_InputField weaponInput;
    public TMP_InputField wingInput;
    public TMP_InputField hatInput;
    public TMP_InputField eyesInput;



    [SerializeField]
    private BasePlayer basePlayer;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ApplyVariants();
        }
    }

    void ApplyVariants()
    {
        PlayerAnimationController characterLoader = basePlayer.GetComponent<PlayerAnimationController>();
        Character character = basePlayer.GetComponent<Character>();


        if (!string.IsNullOrEmpty(outfitInput.text))
        {
            if (int.TryParse(outfitInput.text, out int bodyVariant))
            {
                characterLoader.LoadPart(CharacterPart.Body, bodyVariant);
                characterLoader.LoadPart(CharacterPart.Legs, bodyVariant);
            }
        }

        // Legs
        if (!string.IsNullOrEmpty(hairInput.text))
        {
            if (int.TryParse(hairInput.text, out int hairVariant))
            {
                characterLoader.LoadPart(CharacterPart.Hair, hairVariant);
            }
        }

        if (!string.IsNullOrEmpty(headInput.text))
        {
            if (int.TryParse(headInput.text, out int headVariant))
            {
                characterLoader.LoadPart(CharacterPart.Head, headVariant);
            }
        }

        if (!string.IsNullOrEmpty(wingInput.text))
        {
            if (int.TryParse(wingInput.text, out int wingVariant))
            {
                characterLoader.LoadPart(CharacterPart.Wings, wingVariant);
            }
        }

        if (!string.IsNullOrEmpty(weaponInput.text))
        {
            if (int.TryParse(weaponInput.text, out int weaponVariant))
            {
                characterLoader.LoadPart(character.getWeaponType(), weaponVariant);
            }
        }

        if (!string.IsNullOrEmpty(hatInput.text))
        {
            if (int.TryParse(hatInput.text, out int hatVariant))
            {
                characterLoader.LoadPart(CharacterPart.Hat, hatVariant);
            }
        }

        if (!string.IsNullOrEmpty(eyesInput.text))
        {
            if (int.TryParse(eyesInput.text, out int eyesVariant))
            {
                characterLoader.LoadPart(CharacterPart.Eyes, eyesVariant);
            }
        }
    }
}
