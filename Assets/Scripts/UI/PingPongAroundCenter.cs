using UnityEngine;

public class PingPongAroundCenter : MonoBehaviour
{
    public Vector2 direction = Vector2.right; // hướng chuyển động
    public float distance = 2f;               // tổng độ dài đường đi
    public float speed = 2f;                  // tốc độ qua lại
    public Transform center;                  // nếu null => lấy vị trí bắt đầu (fixed center)

    void Start()
    {
        direction = direction.normalized;
    }

    void Update()
    {
        float t = Mathf.PingPong(Time.time * speed, 1f) - 0.5f;
        Vector3 offset = (Vector3)direction * distance * t;

        transform.localPosition = offset;
    }

}
