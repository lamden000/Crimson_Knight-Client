using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MiniMapCamera : MonoBehaviour
{
    [Header("Targets")]
    public Transform player;
    private Collider2D mapBoundary;

    [Header("Options")]
    public bool followX = true;
    public bool followY = true;
    public float smoothSpeed = 10f;

    private Camera cam;
    private Bounds bounds;
    public static MiniMapCamera instance { get; private set; }

    void Awake()
    {
        cam = GetComponent<Camera>();

        if (mapBoundary != null)
        {
            bounds = mapBoundary.bounds;
        }
        if(instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void InitializeBounds(BoxCollider2D bound)
    {
      /*  if (cam == null)
        {
            cam = GetComponent<Camera>();
        }

        bounds = GameObject.FindGameObjectWithTag("Map Boundary")?.GetComponent<BoxCollider2D>();
        halfHeight = orthographicSize;

        halfWidth = halfHeight * cam.aspect;

        if (bounds != null)
        {
            minBounds = bounds.bounds.min;
            maxBounds = bounds.bounds.max;
        }
        else
        {
            Debug.LogWarning("CameraFollow: Bounds collider is not set!");
        }*/
      mapBoundary= bound;
    }


    void LateUpdate()
    {
        if (player == null || mapBoundary == null)
            return;

        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        Vector3 targetPos = transform.position;

        if (followX)
            targetPos.x = player.position.x;

        if (followY)
            targetPos.y = player.position.y;

        // Clamp trong boundary
        targetPos.x = Mathf.Clamp(
            targetPos.x,
            bounds.min.x + camWidth,
            bounds.max.x - camWidth
        );

        targetPos.y = Mathf.Clamp(
            targetPos.y,
            bounds.min.y + camHeight,
            bounds.max.y - camHeight
        );

        // Gi? nguyên Z (2D mà, ??ng ngh?ch)
        targetPos.z = transform.position.z;

        // Smooth follow (ho?c b? n?u b?n thích gi?t c?c)
        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            smoothSpeed * Time.deltaTime
        );
    }
}
