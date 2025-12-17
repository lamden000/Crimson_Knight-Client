using UnityEngine;

public class PingPongAroundCenter : MonoBehaviour
{
    public Vector2 direction = Vector2.right; // hướng chuyển động
    public float distance = 2f;               // tổng độ dài đường đi
    public float speed = 2f;                  // tốc độ qua lại
    public Transform center;                  // nếu null => lấy vị trí bắt đầu (fixed center)

    private Vector3 fixedCenterPos;           // vị trí tâm cố định (world space)
    private bool useFixedCenter = true;

    void Start()
    {
        direction = direction.normalized;

        if (center == null)
        {
            // không dùng transform làm center reference (tránh cộng dồn)
            fixedCenterPos = transform.position;
            useFixedCenter = true;
        }
        else
        {
            // nếu center được set (không phải chính transform), lấy vị trí động của center mỗi frame
            // bạn có thể muốn dùng vị trí động hoặc cố định của center; ở đây ta theo vị trí động
            useFixedCenter = false;
        }
    }

    void Update()
    {
        float t = Mathf.PingPong(Time.time * speed, 1f) - 0.5f;
        Vector3 offset = (Vector3)direction * distance * t;

        transform.localPosition = offset;
    }

}
