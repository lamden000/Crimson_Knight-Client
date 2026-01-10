using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjFocusInfoUIManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI txtName;
    [SerializeField]
    private TextMeshProUGUI txtLV;
    [SerializeField]
    private GameObject iconHp;
    [SerializeField]
    private TextMeshProUGUI txtContentHp;

    void Update()
    {
        RefreshData();
    }

    void RefreshData()
    {
        if (ClientReceiveMessageHandler.Player.objFocus == null)
        {
            txtName.text = "";
            txtLV.text = "";
            txtContentHp.text = "";
            iconHp.SetActive(false);
        }
        else
        {
            var obj = ClientReceiveMessageHandler.Player.objFocus;
            txtName.text = obj.Name;
            txtLV.text = "LV: " + obj.Level;
            txtContentHp.text = obj.CurrentHp + " / " + obj.MaxHp;
            iconHp.SetActive(true);
        }

        StartCoroutine(RebuildNextFrame());
    }

    IEnumerator RebuildNextFrame()
    {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            txtContentHp.transform.parent as RectTransform
        );
    }

}
