using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public Transform target;              // Nhân vật
    public float smoothSpeed = 5f;        // Độ mượt khi camera theo dõi
    public Vector3 offset = new Vector3(0, 0, -10); // Giữ camera lùi ra sau Z

    void LateUpdate()
    {
        if (target == null) return;

        // Lấy đúng vị trí player, chỉ thay Z = -10
        Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, offset.z);

        // Camera di chuyển mượt theo player
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}
