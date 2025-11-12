using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class NPCMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform contentContainer; 

    public event Action<string> OnOptionSelected;

    public void ShowMenu(NPCMenu menu)
    {
        gameObject.SetActive(true);

        // Xóa nút cũ
        foreach (Transform child in contentContainer)
            Destroy(child.gameObject);

        // Tạo mới từng nút từ prefab
        foreach (var option in menu.options)
        {
            var buttonObj = Instantiate(buttonPrefab, contentContainer);
            buttonObj.SetActive(true);

            var text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            text.text = option.label;

            string actionKey = option.actionKey; // capture local copy
            buttonObj.GetComponent<Button>().onClick.AddListener(() => OnOptionSelected?.Invoke(actionKey));
        }
    }

    public void HideMenu()
    {
        gameObject.SetActive(false);
    }
}
