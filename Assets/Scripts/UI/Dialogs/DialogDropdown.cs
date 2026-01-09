using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class DialogDropdown : DialogBase
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private RectTransform contentParent;
    [SerializeField] private Button itemPrefab;
    [SerializeField] private Button closeButton;
    private Action<int> onSelected;

    public void Setup(string title, string[] options, Action<int> callback)
    {
        titleText.text = title;
        onSelected = callback;

        // 🔹 Xóa các nút cũ
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        // 🔹 Tạo các nút mới
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

        // 🔹 Đặt lại sự kiện đóng
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(Close);

        // 🔹 Cập nhật layout ngay lập tức
        RefreshLayout();
    }

    private void RefreshLayout()
    {
        // Ép Unity cập nhật toàn bộ layout của ScrollView
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);
        contentParent.ForceUpdateRectTransforms();

        // Đảm bảo hoạt động kể cả khi layout chưa cập nhật xong frame này
        StartCoroutine(DelayRebuild());
    }

    private IEnumerator DelayRebuild()
    {
        yield return null; // Đợi 1 frame
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);
    }
}
