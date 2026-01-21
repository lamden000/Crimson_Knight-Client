using UnityEngine;
using UnityEngine.UI;

public class DeadScreenManager : MonoBehaviour
{
    public Button btnRespawn;
    public Button btnRecall;

    private void OnEnable()
    {
        Debug.Log("DeadScreen Enabled");

        btnRespawn.onClick.RemoveAllListeners();
        btnRecall.onClick.RemoveAllListeners();

        btnRespawn.onClick.AddListener(OnClickRespawn);
        btnRecall.onClick.AddListener(OnClickRecall);
    }

    private void OnClickRespawn()
    {
        Debug.Log("Đã nhấn: Respawn");
    }

    private void OnClickRecall()
    {
        Debug.Log("Đã nhấn: Recall");
    }
}
