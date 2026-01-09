using UnityEngine;

public class TestDialog : MonoBehaviour
{

        [SerializeField] private DialogFactory dialogFactory;

        void Start()
        {
            ShowDropdown();
        }

        void ShowDropdown()
        {
            var dialog = dialogFactory.CreateDropdown();

            dialog.gameObject.SetActive(true); // 🔥 BẮT BUỘC

            dialog.Setup(
                "Chọn độ khó",
                new[] { "Dễ", "Bình thường", "Khó", "Ác mộng" },
                OnSelected
            );
        }

        void OnSelected(int index)
        {
            Debug.Log("Đã chọn index = " + index);
        }

}
