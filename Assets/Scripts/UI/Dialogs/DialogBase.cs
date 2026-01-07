using UnityEngine;

public class DialogBase : MonoBehaviour
{
    public virtual void Show()
    {
        gameObject.SetActive(true);
        var rect = GetComponent<RectTransform>();
        Debug.Log($"Dialog pos={rect.anchoredPosition}, scale={rect.localScale}, alpha={(GetComponent<CanvasGroup>()?.alpha ?? 1)}");
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
