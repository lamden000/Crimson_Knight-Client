using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Player Settings")]
    public float moveSpeed = 5f;

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        Vector3 move = new Vector3(moveX, moveY, 0).normalized;
        transform.position += move * moveSpeed * Time.deltaTime;
    }
}
