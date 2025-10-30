using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class TiledMap
{
    public int width;
    public int height;
    public int tilewidth;
    public int tileheight;

    public string orientation;
    public string renderorder;
    public int nextlayerid;
    public int nextobjectid;
    public float version;
    public string tiledversion;

    public List<TiledLayer> layers;
    public List<TiledTileset> tilesets;
}


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
[Serializable]
public class TiledLayer
{
    public int id;
    public string name;
    public string type;
    public bool visible;
    public float opacity;

    public int[] data;
    public int width;
    public int height;


    public List<TiledObject> objects;


    public string draworder;
}

[Serializable]
public class TiledObject
{
    public int id;
    public string name;
    public string type;
    public bool visible;


    public float x;
    public float y;
    public float width;
    public float height;
    public float rotation;

    public int gid;
}

[System.Serializable]
public class TiledTileset
{
    public int firstgid;
    public string source;
}
