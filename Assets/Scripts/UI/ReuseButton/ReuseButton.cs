using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReuseButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform outerRect;
    [SerializeField] private RectTransform innerRect;
    [SerializeField] private Image innerImage;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Button button;

    private const float PADDING = 5f;

    private void Reset()
    {
        outerRect = GetComponent<RectTransform>();
        button = GetComponent<Button>();

        innerImage = transform.Find("Image").GetComponent<Image>();
        innerRect = innerImage.GetComponent<RectTransform>();

        label = GetComponentInChildren<TextMeshProUGUI>();
    }

    // ===== SETUP =====

    public void Setup(
        Vector2 size,
        Vector2 position,
        string text,
        Color innerColor,
        UnityEngine.Events.UnityAction onClick = null
    )
    {
        SetSize(size);
        SetPosition(position);
        SetText(text);
        SetInnerColor(innerColor);
        BindClick(onClick);
    }

    /// <summary>
    /// Setup dùng mã HEX
    /// </summary>
    public void Setup(
        Vector2 size,
        Vector2 position,
        string text,
        string hexColor,
        UnityEngine.Events.UnityAction onClick = null
    )
    {
        SetSize(size);
        SetPosition(position);
        SetText(text);
        SetInnerColorHex(hexColor);
        BindClick(onClick);
    }

    // ===== SETTERS =====

    public void SetSize(Vector2 size)
    {
        outerRect.sizeDelta = size;

        innerRect.sizeDelta = new Vector2(
            size.x - PADDING * 2f,
            size.y - PADDING * 2f
        );
    }

    public void SetPosition(Vector2 anchoredPosition)
    {
        outerRect.anchoredPosition = anchoredPosition;
    }

    public void SetText(string text)
    {
        label.text = text;
    }

    public void SetInnerColor(Color color)
    {
        innerImage.color = color;
    }

    public void SetInnerColorHex(string hex)
    {
        if (!hex.StartsWith("#"))
            hex = "#" + hex;

        if (ColorUtility.TryParseHtmlString(hex, out Color color))
        {
            innerImage.color = color;
        }
        else
        {
            Debug.LogWarning($"[ReuseButton] Invalid HEX color: {hex}");
        }
    }

    private void BindClick(UnityEngine.Events.UnityAction onClick)
    {
        if (onClick == null) return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClick);
    }
}
