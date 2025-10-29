using UnityEngine;

public class StartDialogue : MonoBehaviour
{
    private void OnEnable()
    {
        GameManager.Instance.SetPlayerControl(false);
    }

    private void OnDisable()
    {
        GameManager.Instance.SetPlayerControl(true);
    }
}
