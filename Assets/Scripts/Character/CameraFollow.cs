using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;   // Player
    [SerializeField] private float smoothSpeed = 5f; // t?c ?? m??t
    [SerializeField] private Vector3 offset;     // l?ch tr?c camera

    private void LateUpdate()
    {
        if (target == null) return;

        // V? tr� c?n ??n (theo player + offset)
        Vector3 desiredPosition = target.position + offset;

        // Gi? nguy�n z (camera 2D th??ng ? -10)
        desiredPosition.z = transform.position.z;

        // Di chuy?n m??t ??n v? tr� ?�
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}
