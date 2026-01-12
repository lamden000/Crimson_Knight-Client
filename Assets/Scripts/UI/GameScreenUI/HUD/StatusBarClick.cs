using UnityEngine;
using UnityEngine.EventSystems;

public class StatusBarClick : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log("click statusbar");
            UIManager.Instance.gameScreenUIManager.ShowMenuTab();
        }
    }
}
