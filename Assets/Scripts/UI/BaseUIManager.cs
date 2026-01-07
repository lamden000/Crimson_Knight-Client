using UnityEngine;

public class BaseUIManager : MonoBehaviour
{
    public virtual void ShowUI()
    {
        gameObject.SetActive(true);
    }
    public virtual void HideUI()
    {
        gameObject.SetActive(false);
    }
}
