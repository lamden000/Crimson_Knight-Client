using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class CharacterDialogManager : MonoBehaviour
{
    public static CharacterDialogManager Instance { get; private set; }

    [Header("Dialogue UI References")]
    public Canvas screenCanvas;
    public Canvas worldCanvas;
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Image portraitImage;
    public Button questButton;
    public Button closeButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }
}
