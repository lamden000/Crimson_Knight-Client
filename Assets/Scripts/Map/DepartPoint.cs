using System.IO;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DepartPoint : MonoBehaviour
{
    // The target map name (can be with or without .json). This is set when the depart point is created.
    public string destinationMapName;
    // direction string parsed from Tiled (e.g. "left", "right", "up", "down")
    public string direction = "right";
    // arrow animation settings
    public float arrowPingAmplitude = 0.2f;
    public float arrowPingSpeed = 2f;

    private Transform arrowTransform;
    private Vector3 arrowStartLocalPos;
    private Vector3 arrowDirVector = Vector3.right;
    // distance to offset arrow in the opposite direction so it sits outside the collider
    public float arrowOffsetDistance = 0.3f;

    private void Start()
    {
        // Ensure collider is a trigger
        var col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // Find or create arrow child (expects a child in prefab). Prefer first child if present.
        if (transform.childCount > 0)
            arrowTransform = transform.GetChild(0);
        else
        {
            var go = new GameObject("Arrow");
            go.transform.SetParent(transform, false);
            arrowTransform = go.transform;
        }

        // set direction vector and rotation (default arrow points right)
        switch ((direction ?? "right").ToLowerInvariant())
        {
            case "left":
                arrowDirVector = Vector3.left;
                arrowTransform.localRotation = Quaternion.Euler(0, 0, 180f);
                break;
            case "up":
                arrowDirVector = Vector3.up;
                arrowTransform.localRotation = Quaternion.Euler(0, 0, 90f);
                break;
            case "down":
                arrowDirVector = Vector3.down;
                arrowTransform.localRotation = Quaternion.Euler(0, 0, -90f);
                break;
            default:
                arrowDirVector = Vector3.right;
                arrowTransform.localRotation = Quaternion.identity;
                break;
        }

        // record start local position and apply an offset opposite to the arrow direction
        // so the arrow sits outside the collider (e.g., for "up" arrow we move it down)
        arrowStartLocalPos = arrowTransform.localPosition + (-arrowDirVector) * arrowOffsetDistance;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only respond to player collisions (expects the Player GameObject to have tag "Player")
        if (!other.CompareTag("Player"))
            return;

        var loader = FindAnyObjectByType<GridmapLoader>();
        if (loader == null)
        {
            Debug.LogError("DepartPoint: No GridmapLoader found in scene to handle map transition.");
            return;
        }

        // Use current loader jsonFileName (strip extension) as origin
        string current = loader.jsonFileName ?? string.Empty;
        string originBase = Path.GetFileNameWithoutExtension(current);

        loader.LoadMapByName(destinationMapName, originBase);
    }

    private void Update()
    {
        if (arrowTransform == null) return;
        float offset = Mathf.Sin(Time.time * arrowPingSpeed) * arrowPingAmplitude;
        arrowTransform.localPosition = arrowStartLocalPos + arrowDirVector * offset;
    }
}


