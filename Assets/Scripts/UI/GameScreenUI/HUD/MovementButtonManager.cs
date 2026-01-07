using UnityEngine;

public class MovementButtonManager : MonoBehaviour
{
    public ButtonPressEffect upButton;
    public ButtonPressEffect leftButton;
    public ButtonPressEffect downButton;
    public ButtonPressEffect rightButton;

    void Update()
    {
        // W
        if (Input.GetKeyDown(KeyCode.W)) upButton.Press();
        if (Input.GetKeyUp(KeyCode.W)) upButton.Release();

        // A
        if (Input.GetKeyDown(KeyCode.A)) leftButton.Press();
        if (Input.GetKeyUp(KeyCode.A)) leftButton.Release();

        // S
        if (Input.GetKeyDown(KeyCode.S)) downButton.Press();
        if (Input.GetKeyUp(KeyCode.S)) downButton.Release();

        // D
        if (Input.GetKeyDown(KeyCode.D)) rightButton.Press();
        if (Input.GetKeyUp(KeyCode.D)) rightButton.Release();
    }
}
