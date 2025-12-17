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

    public GameObject minimapWindow;

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
      bounds = bound.bounds;
    }

    void LateUpdate()
    {
        if (player == null || mapBoundary == null)
            return;

        bounds = mapBoundary.bounds;

        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        Vector3 targetPos = transform.position;

        if (followX) targetPos.x = player.position.x;
        if (followY) targetPos.y = player.position.y;

        float minX = bounds.min.x + camWidth;
        float maxX = bounds.max.x - camWidth;
        float minY = bounds.min.y + camHeight;
        float maxY = bounds.max.y - camHeight;

        if (minX > maxX)
            targetPos.x = bounds.center.x;
        else
            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);

        if (minY > maxY)
            targetPos.y = bounds.center.y;
        else
            targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);

        targetPos.z = transform.position.z;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            smoothSpeed * Time.deltaTime
        );
    }

}
