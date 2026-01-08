using UnityEngine;

public class MovementButtonManager : MonoBehaviour
{
    public ButtonPressEffect upButton;
    public ButtonPressEffect leftButton;
    public ButtonPressEffect downButton;
    public ButtonPressEffect rightButton;

    public static Vector2 MovementInput { get; private set; }

    private void Start()
    {
        if (Main.IsPC())
        {
            this.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (rightButton != null && rightButton.isPressed)
        {
            horizontal = 1f;
        }
        else if (leftButton != null && leftButton.isPressed)
        {
            horizontal = -1f;
        }

        if (upButton != null && upButton.isPressed)
        {
            vertical = 1f;
        }
        else if (downButton != null && downButton.isPressed)
        {
            vertical = -1f;
        }

        MovementInput = new Vector2(horizontal, vertical);
    }
}
