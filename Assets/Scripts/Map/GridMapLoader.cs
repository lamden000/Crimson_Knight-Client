using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Tilemaps;

[ExecuteAlways]
[RequireComponent(typeof(Pathfinder))]
public class GridmapLoader : MonoBehaviour
{
    public string jsonFileName = "map1.json";
    public Tilemap tilemap;  
    public float tileScale = 1f;

    private Dictionary<int, Tile> gidToTile = new Dictionary<int, Tile>();

    public bool loadInEditMode = false;

    private void Start()
    {
        if (Application.isPlaying)
            StartCoroutine(LoadJsonFile(jsonFileName));
    }

    private void Update()
    {
#if UNITY_EDITOR
        // Nếu đang ở Edit Mode và user bật toggle lên
        if (!Application.isPlaying && loadInEditMode)
        {
            loadInEditMode = false; // Tắt toggle để tránh load liên tục
            StartCoroutine(LoadJsonFile(jsonFileName));
        }
#endif
    }

    IEnumerator LoadJsonFile(string jsonFileName)
    {

        string jsonPath = Path.Combine(Application.streamingAssetsPath, jsonFileName);

        string jsonPathUri = new System.Uri(Path.Combine(Application.streamingAssetsPath, jsonFileName)).AbsoluteUri;

        using (UnityWebRequest www = UnityWebRequest.Get(jsonPathUri))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Loi khi tai file tu streamingAssets: {www.error} tai duong dan: {jsonPathUri}");
            }
            else
            {
                string jsonText = www.downloadHandler.text;
                TiledMap map = JsonUtility.FromJson<TiledMap>(jsonText);

                foreach (var ts in map.tilesets)
                    LoadTileset(ts);

                LoadLayers(map);
            }
        }
    }

    void LoadTileset(TiledTileset ts)
    {
        int firstGid = ts.firstgid;

        string folder = Path.GetFileNameWithoutExtension(ts.source); 

        for (int id = 0; ; id++)
        {
            string resourcePath = $"Tiles/{folder}/{id + 1}";

            Sprite sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite == null)
            {
                if (id == 0)
                    Debug.LogWarning($"Không tìm thấy tile nào trong {resourcePath}");
                break;
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
        TileNode[,] gridNodes = new TileNode[height, width];
        foreach (var layer in map.layers)
        {
            if (layer.type != "tilelayer") continue;
            bool isCollisionLayer = layer.name.ToLower().Contains("block");
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
                int logicY = i / width; 
                int drawY = height - 1 - logicY;
                Vector3Int pos = new Vector3Int(x, drawY, 0);
                Vector3 worldPos = tilemap.CellToWorld(pos);
                //        Debug.Log($"[{layer.name}] Tile {gid} at grid({x},{y}) -> world({worldPos.x:F2}, {worldPos.y:F2}, {worldPos.z:F2}) | H:{flipH} V:{flipV} D:{flipD}");
                Tile tile = gidToTile[gid];
                Matrix4x4 matrix = Matrix4x4.identity;

                if (flipD)
                {
                    if (flipH && flipV) matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 0, 270));       // H+V+D
                    else if (flipH && !flipV) matrix = Matrix4x4.Scale(new Vector3(-1, 1, 1)) *  Matrix4x4.Scale(new Vector3(1, -1, 1))* Matrix4x4.Rotate(Quaternion.Euler(0, 0, 90)) ; // H+D
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
                if (gridNodes[logicY, x] == null)
                    gridNodes[logicY, x] = new TileNode(x, logicY, worldPos, true);
                if (isCollisionLayer)
                    gridNodes[logicY, x].walkable = false;
            }
        }
        Pathfinder.Instance.Init(gridNodes);
       //Debug.Log("World:"+gridNodes[0, 0].worldPos+" Grid:"+ gridNodes[0, 0].gridPos);
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
