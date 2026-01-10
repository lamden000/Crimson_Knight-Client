using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonPressEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float pressedScale = 0.9f;
    public float animationSpeed = 10f;

    private Vector3 originalScale;
    public bool isPressed = false; 

    void Start()
    {
        originalScale = transform.localScale;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        Press();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Release();
    }

    public void Press()
    {
        isPressed = true;
    }

    public void Release()
    {
        isPressed = false;
    }

    void Update()
    {
        if (isPressed)
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                originalScale * pressedScale,
                Time.deltaTime * animationSpeed
            );
        }
        else
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                originalScale,
                Time.deltaTime * animationSpeed
            );
        }
    }
}
