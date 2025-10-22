using UnityEngine;
public class TileNode
{
    public Vector2Int gridPos;
    public Vector3 worldPos;
    public bool walkable;

    public TileNode(int x, int y, Vector3 worldPos, bool walkable = true)
    {
        this.gridPos = new Vector2Int(x, y);
        this.worldPos = worldPos;
        this.walkable = walkable;
    }
}
