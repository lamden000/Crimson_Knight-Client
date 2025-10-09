using UnityEngine;

using UnityEngine.U2D; 

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(PixelPerfectCamera))]
public class CameraFollow : MonoBehaviour
{
    public Transform target;               // Player hoặc object cần theo dõi
    public float smoothSpeed = 5f;         // Tốc độ di chuyển camera
    public BoxCollider2D bounds;           // Vùng giới hạn camera

    private float halfHeight;
    private float halfWidth;
    private Vector3 minBounds;
    private Vector3 maxBounds;
    private PixelPerfectCamera ppc;

    private void Start()
    {
        Camera cam = GetComponent<Camera>();
        ppc = GetComponent<PixelPerfectCamera>();

        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;

        if (bounds != null)
        {
            minBounds = bounds.bounds.min;
            maxBounds = bounds.bounds.max;
        }
    }

    private void FixedUpdate()
    {
        if (target == null) return;

        // Camera theo dõi player
        Vector3 targetPos = Vector3.Lerp(transform.position, target.position, smoothSpeed * Time.deltaTime);

        // Giới hạn trong BoxCollider
        if (bounds != null)
        {
            float clampX = Mathf.Clamp(targetPos.x, minBounds.x + halfWidth, maxBounds.x - halfWidth);
            float clampY = Mathf.Clamp(targetPos.y, minBounds.y + halfHeight, maxBounds.y - halfHeight);
            targetPos = new Vector3(clampX, clampY, transform.position.z);
        }
        else
        {
            targetPos.z = transform.position.z;
        }

        // Snap theo Pixel Perfect Camera
        if (ppc != null && ppc.refResolutionX > 0) // kiểm tra PPU > 0
        {
            float unitsPerPixel = 1f / ppc.assetsPPU;
            targetPos.x = Mathf.Round(targetPos.x / unitsPerPixel) * unitsPerPixel;
            targetPos.y = Mathf.Round(targetPos.y / unitsPerPixel) * unitsPerPixel;
        }

        transform.position = targetPos;
    }
}

