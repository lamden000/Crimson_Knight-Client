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
    private Vector3 lastBoundsCenter;
    private Vector3 lastBoundsSize;


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
        if (target == null || bounds == null) return;

        // Check if the bounds moved or resized
        if (bounds.bounds.center != lastBoundsCenter || bounds.bounds.size != lastBoundsSize)
        {
            minBounds = bounds.bounds.min;
            maxBounds = bounds.bounds.max;
            lastBoundsCenter = bounds.bounds.center;
            lastBoundsSize = bounds.bounds.size;
        }

        Vector3 targetPos = Vector3.Lerp(transform.position, target.position, smoothSpeed * Time.deltaTime);

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        targetPos.x = Mathf.Clamp(targetPos.x, minBounds.x + halfWidth, maxBounds.x - halfWidth);
        targetPos.y = Mathf.Clamp(targetPos.y, minBounds.y + halfHeight, maxBounds.y - halfHeight);

        transform.position = new Vector3(targetPos.x, targetPos.y, transform.position.z);
    }

}