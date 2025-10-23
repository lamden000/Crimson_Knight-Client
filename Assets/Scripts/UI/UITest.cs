using UnityEngine;

public class UITest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("[UITest] Start() called. UIManager.Instance = " + (UIManager.Instance != null));
        // Test hộp thoại Yes/No
        // UIManager.Instance.ShowYesNo("Bạn có chắc muốn xóa vật phẩm?", 5, (yes, id) =>
        // {
        //     if (yes)
        //         Debug.Log($"Yes được nhấn ở dialog {id}");
        //     else
        //         Debug.Log($"No được nhấn ở dialog {id}");
        // });


        // Test Dropdown
        // string[] options = { "Kiếm", "Khiên", "Cung", "Gậy phép", "Kiếm11", "Khiên12", "Cung12", "Gậy phép12" };

        // UIManager.Instance.ShowDropdown("Chọn vũ khí:", options, index =>
        // {
        //     Debug.Log($"Bạn đã chọn: {index} - {options[index]}");
        // });

        // Test OK Dialog

        UIManager.Instance.ShowOK("Chúc mừng bạn đã chiến thắng!", () =>
        {
            Debug.Log("Đã nhấn OK");
        });

    }
}
