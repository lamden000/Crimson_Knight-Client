using UnityEngine;

public class PingPongAroundCenter : MonoBehaviour
{
    public Vector2 direction = Vector2.right;   // hướng ping pong
    public float distance = 2f;                 // độ dài đường đi
    public float speed = 2f;                    // tốc độ
    private Transform center;                    // tâm chuyển động (mặc định chính object)

    private Vector3 startOffset;

    void Start()
    {
        if (center == null) center = transform;
        direction = direction.normalized;

        // vị trí bắt đầu so với center
        startOffset = direction * distance * 0.5f;
    }

    void Update()
    {
        float t = Mathf.PingPong(Time.time * speed, 1f) - 0.5f; // trả về -0.5 đến +0.5
        Vector3 offset = direction * distance * t;
        transform.position = center.position + offset;
    }
}
