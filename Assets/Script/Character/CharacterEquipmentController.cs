using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterEquipmentController : MonoBehaviour
{
    [Header("References")]
    public TMP_InputField outfitInput;
    public TMP_InputField hairInput;
    public TMP_InputField headInput;

    public AnimationController characterLoader; // script cũ của cậu để load sprites

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

        // Head (nếu có)
        if (!string.IsNullOrEmpty(headInput.text))
        {
            if (int.TryParse(headInput.text, out int headVariant))
            {
                characterLoader.SetVariant(CharacterPart.Head, headVariant);
            }
        }
    }
}
