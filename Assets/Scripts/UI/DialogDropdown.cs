using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DialogDropdown : DialogBase
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Transform contentParent;  
    [SerializeField] private Button itemPrefab;        
    [SerializeField] private Button closeButton;      
    private Action<int> onSelected;

    public void Setup(string title, string[] options, Action<int> callback)
    {
        titleText.text = title;
        onSelected = callback;

        // Xóa nút cũ
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        // Tạo nút mới
        for (int i = 0; i < options.Length; i++)
        {
            int index = i;
            Button item = Instantiate(itemPrefab, contentParent);

            var label = item.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = options[i];

            item.onClick.RemoveAllListeners();
            item.onClick.AddListener(() =>
            {
                onSelected?.Invoke(index);
                Close();
            });
        }
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(Close);
        Canvas.ForceUpdateCanvases();
        var rect = contentParent as RectTransform;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
    }

}
