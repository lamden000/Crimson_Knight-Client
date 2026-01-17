using UnityEngine;
using UnityEngine.UI;

public class FlagBarManager : MonoBehaviour
{
    public Button btnRed;
    public Button btnBlue;
    public Button btnGreen;
    public Button btnYellow;

    private void Start()
    {
        btnRed.onClick.AddListener(() => OnFlagClick("Red"));
        btnBlue.onClick.AddListener(() => OnFlagClick("Blue"));
        btnGreen.onClick.AddListener(() => OnFlagClick("Green"));
        btnYellow.onClick.AddListener(() => OnFlagClick("Yellow"));
    }

    private void OnFlagClick(string flag)
    {
        Debug.Log("Flag selected: " + flag);
    }
}
