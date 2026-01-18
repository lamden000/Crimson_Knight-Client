using Assets.Scripts.Networking;
using UnityEngine;
using UnityEngine.UI;

public class FlagBarManager : MonoBehaviour
{
    public Button btnRed;
    public Button btnBlue;
    public Button btnGreen;
    public Button btnYellow;
    public Button btnNone;

    private void Start()
    {
        btnRed.onClick.AddListener(() => OnFlagClick("Red"));
        btnBlue.onClick.AddListener(() => OnFlagClick("Blue"));
        btnGreen.onClick.AddListener(() => OnFlagClick("Green"));
        btnYellow.onClick.AddListener(() => OnFlagClick("Yellow"));
        btnNone.onClick.AddListener(() => OnFlagClick("None"));
    }

    private void OnFlagClick(string flag)
    {
        if(flag == "Red")
        {
            RequestManager.ChangePkType((PkType)1);
        }
        else if(flag == "Blue")
        {
            RequestManager.ChangePkType((PkType)3);
        }
        else if(flag == "Green")
        {
            RequestManager.ChangePkType((PkType)2);
        }
        else if(flag == "Yellow")
        {
            RequestManager.ChangePkType((PkType)4);
        }
        else
        {
            RequestManager.ChangePkType(0);
        }
    }
}
