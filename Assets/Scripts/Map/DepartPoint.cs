using System.IO;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DepartPoint : MonoBehaviour
{
    // The target map name (can be with or without .json). This is set when the depart point is created.
    public string destinationMapName;

    private void Start()
    {
        // Ensure collider is a trigger
        var col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
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
}


