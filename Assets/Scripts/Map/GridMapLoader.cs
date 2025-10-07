using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Xml;


public class GridmapLoader : MonoBehaviour
{
    public string jsonFileName = "map.json";
    public Tilemap tilemap;  // Kéo thả Tilemap từ scene
    public float tileScale = 1f;

    private Dictionary<int, Tile> gidToTile = new Dictionary<int, Tile>();

    void Start()
    {
        string jsonPath = Path.Combine(Application.streamingAssetsPath, jsonFileName);
        string jsonText = File.ReadAllText(jsonPath);
        TiledMap map = JsonUtility.FromJson<TiledMap>(jsonText);

        // Load tất cả tileset
        foreach (var ts in map.tilesets)
        {
            LoadTileset(ts);
        }

        // Load từng layer
        LoadLayers(map);
    }

    void LoadTileset(TiledTileset ts)
    {
        string tsxPath = Path.Combine(Application.streamingAssetsPath, ts.source);
        XmlDocument doc = new XmlDocument();
        doc.Load(tsxPath);

        XmlNodeList tileNodes = doc.GetElementsByTagName("tile");
        int firstGid = ts.firstgid;

        foreach (XmlNode tileNode in tileNodes)
        {
            int id = int.Parse(tileNode.Attributes["id"].Value);
            XmlNode imageNode = tileNode.SelectSingleNode("image");
            string imgPath = imageNode.Attributes["source"].Value;

            string folder = Path.GetFileName(Path.GetDirectoryName(imgPath));
            string fileName = Path.GetFileNameWithoutExtension(imgPath);
            string resourcePath = $"Tiles/{folder}/{fileName}";

            Sprite sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite == null)
            {
                Debug.LogWarning($"Sprite not found: {resourcePath}");
                continue;
            }

            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            gidToTile[firstGid + id] = tile;
        }
    }

    void LoadLayers(TiledMap map)
    {
        int width = map.width;
        int height = map.height;

        foreach (var layer in map.layers)
        {
            if (layer.type != "tilelayer") continue;

            for (int i = 0; i < layer.data.Length; i++)
            {
                uint rawGid = (uint)layer.data[i];
                if (rawGid == 0) continue;

                bool flipH = (rawGid & 0x80000000) != 0;
                bool flipV = (rawGid & 0x40000000) != 0;
                bool flipD = (rawGid & 0x20000000) != 0;

                int gid = (int)(rawGid & 0x1FFFFFFF); // bỏ 3 flag bits

                if (!gidToTile.ContainsKey(gid)) continue;

                int x = i % width;
                int y = height - 1 - (i / width);
                Vector3Int pos = new Vector3Int(x, y, 0);
                Vector3 worldPos = tilemap.CellToWorld(pos);
                Debug.Log($"[{layer.name}] Tile {gid} at grid({x},{y}) -> world({worldPos.x:F2}, {worldPos.y:F2}, {worldPos.z:F2}) | H:{flipH} V:{flipV} D:{flipD}");
                Tile tile = gidToTile[gid];
                Matrix4x4 matrix = Matrix4x4.identity;

                if (flipD)
                {
                    if (flipH && flipV) matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 0, 270));       // H+V+D
                    else if (flipH && !flipV) matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 0, 90)) * Matrix4x4.Scale(new Vector3(1, -1, 1)); // H+D
                    else if (!flipH && flipV) matrix = Matrix4x4.Scale(new Vector3(-1, 1, 1)) * Matrix4x4.Scale(new Vector3(1, -1, 1)) * Matrix4x4.Rotate(Quaternion.Euler(0, 0, 270)); // V+D
                    else matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 0, 90)); // D only
                }
                else
                {
                    if (flipH) matrix *= Matrix4x4.Scale(new Vector3(-1, 1, 1));
                    if (flipV) matrix *= Matrix4x4.Scale(new Vector3(1, -1, 1));
                }

                tilemap.SetTile(pos, tile);
                tilemap.SetTransformMatrix(pos, matrix);
            }
        }
    }
}
[System.Serializable]
public class TiledMap
{
    public int width;
    public int height;
    public TiledLayer[] layers;
    public TiledTileset[] tilesets;
}

[System.Serializable]
public class TiledLayer
{
    public string name;
    public string type;
    public int[] data;
}

[System.Serializable]
public class TiledTileset
{
    public int firstgid;
    public string source;
}
