using UnityEngine;

public class ReuseButtonTester : MonoBehaviour
{
    [SerializeField] private ReuseButton buttonPrefab;
    [SerializeField] private Transform parent;

    private void Start()
    {
        CreateButton(
            new Vector2(240, 70),
            new Vector2(0, 100),
            "COMMON",
            "#4CAF50"
        );

        CreateButton(
            new Vector2(280, 80),
            new Vector2(0, 0),
            "RARE",
            "#2196F3FF"
        );

        CreateButton(
            new Vector2(300, 90),
            new Vector2(0, -120),
            "LEGENDARY",
            "FFD700"
        );
    }

    private void CreateButton(
        Vector2 size,
        Vector2 position,
        string text,
        string hexColor
    )
    {
        ReuseButton btn = Instantiate(buttonPrefab, parent);

        btn.Setup(
            size,
            position,
            text,
            hexColor,
            () => Debug.Log("Clicked: " + text)
        );
    }
}
