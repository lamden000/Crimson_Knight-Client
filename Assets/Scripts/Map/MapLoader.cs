using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;
using System;
using System.Linq;
using System.Xml;
using System.Collections.Generic;

[System.Serializable]
public class Chunk
{
    public string data;
    public int height;
    public int width;
    public int x;
    public int y;
}

[System.Serializable]
public class Layer
{
    public string name;
    public string type;
    public string encoding;
    public string compression;
    public Chunk[] chunks;
}

[System.Serializable]
public class MapData
{
    public int tilewidth;
    public int tileheight;
    public Layer[] layers;
    public TilesetRef[] tilesets;
}

[System.Serializable]
public class TilesetRef
{
    public int firstgid;
    public string source;
}

public class MapLoader : MonoBehaviour
{
    [Header("Unity References")]
    public Tilemap tilemap;
    public string mapFileName = "MapTest..json";
    public string tilesFolder = "Tiles";

    private Dictionary<int, Sprite> gidToSprite = new Dictionary<int, Sprite>();
    private Dictionary<int, Tile> _tileCache = new Dictionary<int, Tile>();

    void Start()
    {
        LoadMap();
    }

    void LoadMap()
    {
        string path = Path.Combine(Application.streamingAssetsPath, mapFileName);
        if (!File.Exists(path))
        {
            Debug.LogError("Không tìm thấy file JSON: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        MapData mapData = JsonUtility.FromJson<MapData>(json);

        if (mapData?.layers == null || mapData.layers.Length == 0)
        {
            Debug.LogError("JSON không hợp lệ hoặc không có layer.");
            return;
        }

        // load tileset mapping từ tsx
        foreach (var ts in mapData.tilesets)
        {
            LoadTilesetMapping(ts.source, ts.firstgid);
        }

        GenerateMap(mapData);
    }

    void GenerateMap(MapData mapData)
    {
        tilemap.ClearAllTiles();
        _tileCache.Clear();

        foreach (var layer in mapData.layers)
        {
            if (layer.type != "tilelayer" || layer.chunks == null) continue;

            foreach (var chunk in layer.chunks)
            {
                int[] tileIDs = DecodeBase64(chunk.data);

                int index = 0;
                for (int y = 0; y < chunk.height; y++)
                {
                    for (int x = 0; x < chunk.width; x++)
                    {
                        int gid = tileIDs[index++];
                        if (gid <= 0) continue;

                        Sprite sprite = GetSpriteByGid(gid);
                        if (sprite == null) continue;

                        Tile tile = GetTile(gid, mapData.tilewidth, sprite);
                        Vector3Int pos = new Vector3Int(chunk.x + x, -(chunk.y + y), 0);
                        tilemap.SetTile(pos, tile);
                    }
                }
            }
        }

        tilemap.RefreshAllTiles();
    }

    Tile GetTile(int gid, int tileWidth, Sprite sprite)
    {
        if (_tileCache.TryGetValue(gid, out Tile cached)) return cached;

        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        /* float scale = 1f / tileWidth;
         tile.transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1));*/
        tile.transform = Matrix4x4.identity;
        tile.flags = TileFlags.LockTransform;

        _tileCache[gid] = tile;
        return tile;
    }

    // ==================== TSX Loader ====================
    void LoadTilesetMapping(string tsxPath, int firstGid)
    {
        string fileName = Path.GetFileNameWithoutExtension(tsxPath);
        string tsxFullPath = Path.Combine(Application.streamingAssetsPath, tsxPath);
        if (!File.Exists(tsxFullPath))
        {
            Debug.LogError("Không tìm thấy TSX: " + tsxFullPath);
            return;
        }

        XmlDocument xml = new XmlDocument();
        xml.LoadXml(File.ReadAllText(tsxFullPath));

        foreach (XmlNode tileNode in xml.SelectNodes("//tile"))
        {
            int id = int.Parse(tileNode.Attributes["id"].Value);
            int gid = id + firstGid;

            XmlNode imgNode = tileNode.SelectSingleNode("image");
            string source = imgNode.Attributes["source"].Value;
            string fileNameNoExt = Path.GetFileNameWithoutExtension(source);

            Sprite sprite = Resources.Load<Sprite>($"{tilesFolder}/{fileNameNoExt}");
            if (sprite != null)
            {
                gidToSprite[gid] = sprite;
            }
            else
            {
                Debug.LogWarning($"Không tìm thấy sprite: {fileNameNoExt}");
            }
        }
    }

    Sprite GetSpriteByGid(int gid)
    {
        return gidToSprite.ContainsKey(gid) ? gidToSprite[gid] : null;
    }

    // ==================== Decode Base64 ====================
    int[] DecodeBase64(string base64)
    {
        byte[] bytes = Convert.FromBase64String(base64);
        int[] result = new int[bytes.Length / 4];
        for (int i = 0; i < result.Length; i++)
            result[i] = BitConverter.ToInt32(bytes, i * 4);
        return result;
    }
}
