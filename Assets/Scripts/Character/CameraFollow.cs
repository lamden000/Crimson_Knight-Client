using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public BoxCollider2D bounds;

    private float halfHeight;
    private float halfWidth;
    private Vector3 minBounds;
    private Vector3 maxBounds;
    private Camera cam;

    public void InitializeBounds(float calculatedOrthographicSize)
    {
        if (cam == null)
        {
            cam = GetComponent<Camera>();
        }
        halfHeight = calculatedOrthographicSize;

        halfWidth = halfHeight * cam.aspect;

        if (bounds != null)
        {
            minBounds = bounds.bounds.min;
            maxBounds = bounds.bounds.max;
        }
        else
        {
            Debug.LogWarning("CameraFollow: Bounds collider is not set!");
        }
    }
    private void LateUpdate()
    {
        if (target == null) return;

        if (bounds != null && halfWidth == 0)
        {
            float calculatedOrthoSize = 540f / (1f * 2f);
            InitializeBounds(calculatedOrthoSize);
        }

        Vector3 targetPos = Vector3.Lerp(transform.position, target.position, smoothSpeed * Time.deltaTime);

        if (bounds != null)
        {
            float clampX = Mathf.Clamp(targetPos.x, minBounds.x + halfWidth, maxBounds.x - halfWidth);
            float clampY = Mathf.Clamp(targetPos.y, minBounds.y + halfHeight, maxBounds.y - halfHeight);
            targetPos.x = clampX;
            targetPos.y = clampY;
        }

        transform.position = new Vector3(targetPos.x, targetPos.y, transform.position.z);
    }
}