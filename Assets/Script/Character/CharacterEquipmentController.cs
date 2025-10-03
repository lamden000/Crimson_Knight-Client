using UnityEngine;
using UnityEngine.UI;
using TMPro;

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



    public PlayerAnimationController characterLoader; // script cũ của cậu để load sprites

    void Update()
    {
        // Nhấn Enter để apply
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ApplyVariants();
        }
    }

    void ApplyVariants()
    {
        // Body
        if (!string.IsNullOrEmpty(outfitInput.text))
        {
            if (int.TryParse(outfitInput.text, out int bodyVariant))
            {
                characterLoader.SetVariant(CharacterPart.Body, bodyVariant);
                characterLoader.SetVariant(CharacterPart.Legs, bodyVariant);
            }
        }

        // Legs
        if (!string.IsNullOrEmpty(hairInput.text))
        {
            if (int.TryParse(hairInput.text, out int hairVariant))
            {
                characterLoader.SetVariant(CharacterPart.Hair, hairVariant);
            }
        }

        if (!string.IsNullOrEmpty(headInput.text))
        {
            if (int.TryParse(headInput.text, out int headVariant))
            {
                characterLoader.SetVariant(CharacterPart.Head, headVariant);
            }
        }

        if (!string.IsNullOrEmpty(wingInput.text))
        {
            if (int.TryParse(wingInput.text, out int wingVariant))
            {
                characterLoader.SetVariant(CharacterPart.Wings, wingVariant);
            }
        }

        if (!string.IsNullOrEmpty(weaponInput.text))
        {
            if (int.TryParse(weaponInput.text, out int weaponVariant))
            {
                characterLoader.SetVariant(CharacterPart.Sword, weaponVariant);
            }
        }

        if (!string.IsNullOrEmpty(hatInput.text))
        {
            if (int.TryParse(hatInput.text, out int hatVariant))
            {
                characterLoader.SetVariant(CharacterPart.Hat, hatVariant);
            }
        }

        if (!string.IsNullOrEmpty(eyesInput.text))
        {
            if (int.TryParse(eyesInput.text, out int eyesVariant))
            {
                characterLoader.SetVariant(CharacterPart.Eyes, eyesVariant);
            }
        }
    }
}
