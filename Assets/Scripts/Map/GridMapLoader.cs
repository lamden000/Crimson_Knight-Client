using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public float offsetCorrectionY= 200;
    private Pathfinder pathfinder;
    private TiledMap map;
    public bool drawGizmo=false;
    private void Start()
    {
        pathfinder=Pathfinder.Instance;
        if (Application.isPlaying)
            StartCoroutine(LoadJsonFile(jsonFileName));
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && loadInEditMode)
        {
            loadInEditMode = false;
            StartCoroutine(LoadJsonFile(jsonFileName));
        }
#endif
    }

    IEnumerator LoadJsonFile(string jsonFileName)
    {
        string jsonPath = Path.Combine(Application.streamingAssetsPath, jsonFileName);
        string jsonPathUri = new System.Uri(jsonPath).AbsoluteUri;

        using (UnityWebRequest www = UnityWebRequest.Get(jsonPathUri))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"❌ Lỗi khi tải file từ streamingAssets: {www.error} tại {jsonPathUri}");
                yield break;
            }

            string jsonText = www.downloadHandler.text;
            map = JsonUtility.FromJson<TiledMap>(jsonText);

            if (map == null)
            {
                Debug.LogError($"❌ Không parse được JSON: {jsonFileName}");
                yield break;
            }

            // 🔹 B1: Quét GID được sử dụng trong cả tilelayer & objectlayer
            HashSet<int> usedGids = new HashSet<int>();
            foreach (var layer in map.layers)
            {
                if (layer.type == "tilelayer" && layer.data != null)
                {
                    foreach (int raw in layer.data)
                    {
                        int gid = (int)(raw & 0x1FFFFFFF);
                        if (gid > 0) usedGids.Add(gid);
                    }
                }
                else if (layer.type == "objectgroup" && layer.objects != null)
                {
                    foreach (var obj in layer.objects)
                    {
                        if (obj.gid > 0) usedGids.Add(obj.gid);
                    }
                }
            }

            Debug.Log($"[GridmapLoader] 🧩 Tổng cộng {usedGids.Count} GID được dùng.");

            // 🔹 B2: Load các tileset cần thiết
            foreach (var ts in map.tilesets)
                LoadTileset(ts, usedGids);

            // 🔹 B3: Load tile layer
            LoadTileLayers();

            // 🔹 B4: Load object layer
            LoadObjectLayers();

            Debug.Log("✅ Map loaded hoàn tất.");
        }
    }

    // ==========================================================
    // 🔹 LOAD TILESET
    // ==========================================================
    void LoadTileset(TiledTileset ts, HashSet<int> usedGids)
    {
        int firstGid = ts.firstgid;
        string folder = Path.GetFileNameWithoutExtension(ts.source);

        if (!usedGids.Any(gid => gid >= firstGid))
            return;

        int loadedCount = 0;

        foreach (int gid in usedGids)
        {
            if (gid < firstGid) continue;

            int localId = gid - firstGid;
            string resourcePath = $"Tiles/{folder}/{localId + 1}";
            Sprite sprite = Resources.Load<Sprite>(resourcePath);

            if (sprite == null)
                continue;

            if (!gidToTile.ContainsKey(gid))
            {
                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sprite;
                gidToTile[gid] = tile;
                loadedCount++;
            }
        }
    }

    void LoadTileLayers()
    {
        int width = map.width;
        int height = map.height;

        TileNode[,] gridNodes = new TileNode[height, width];

        foreach (var layer in map.layers)
        {
            if (layer.type != "tilelayer" || layer.data == null)
                continue;

            for (int i = 0; i < layer.data.Length; i++)
            {
                uint rawGid = (uint)layer.data[i];
                if (rawGid == 0) continue;

                int gid = (int)(rawGid & 0x1FFFFFFF);
                if (!gidToTile.ContainsKey(gid)) continue;

                int x = i % width;
                int logicY = i / width;
                int drawY = height - 1 - logicY;

                Vector3Int pos = new Vector3Int(x, drawY, 0);
                Tile tile = gidToTile[gid];

                tilemap.SetTile(pos, tile);

                if (gridNodes[logicY, x] == null)
                {
                    Vector3 worldPos = tilemap.CellToWorld(pos);
                    gridNodes[logicY, x] = new TileNode(x, logicY, worldPos, true);
                }
            }
        }

        // Sau khi set tile → xử lý collider
        ApplyObjectCollidersToGrid(gridNodes);

        Pathfinder.Instance.Init(gridNodes);
    }


    void LoadObjectLayers()
    {
        foreach (var layer in map.layers)
        {
            if (layer.type != "objectgroup" || layer.objects == null)
                continue;

            foreach (var obj in layer.objects)
            {
                // 🔸 Nếu là collider → tạo box collider
                if (obj.type == "Collider")
                {
                    CreateColliderBox(obj);
                    continue;
                }

                // 🔸 Nếu là object có sprite
                if (obj.gid > 0 && gidToTile.TryGetValue(obj.gid, out Tile tile))
                {
                    GameObject go = new GameObject($"Object_{obj.id}");
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sortingOrder = 5;
                    sr.sprite = tile.sprite;

                    float mapHeightInWorldUnits = map.height * map.tileheight;
                    float centerX = obj.x + obj.width / 2f;
                    float centerY = (mapHeightInWorldUnits - obj.y) - obj.height / 2f;
                    centerY += offsetCorrectionY;
                    go.transform.position = new Vector3(centerX, centerY, 0);
                    float spritePixelWidth = sr.sprite.bounds.size.x;
                    float spritePixelHeight = sr.sprite.bounds.size.y;

                    go.transform.localScale = new Vector3(
                        obj.width / spritePixelWidth,
                        obj.height / spritePixelHeight,
                        1
                    );


                    Debug.Log($"[Object] {obj.id} GID={obj.gid} Pos=({obj.x:F1},{obj.y:F1}) Size=({obj.width}x{obj.height}) Sprite={sr.sprite?.name}");
                }
            }
        }
    }

    void CreateColliderBox(TiledObject obj)
    {
        GameObject go = new GameObject("ColliderBox");
        var col = go.AddComponent<BoxCollider2D>();

        float centerX = obj.x + obj.width / 2f;
        float mapHeightInWorldUnits = map.height * map.tileheight;
        float centerY = (mapHeightInWorldUnits - obj.y) - obj.height / 2f;

        go.transform.position = new Vector3(centerX, centerY, 0);

        col.size = new Vector2(obj.width, obj.height);

        col.offset = Vector2.zero;

        go.transform.SetParent(transform);
    }

    private void ApplyObjectCollidersToGrid( TileNode[,] gridNodes)
    {
        int gridHeight = gridNodes.GetLength(0);
        int gridWidth = gridNodes.GetLength(1);
        float tileW = map.tilewidth;
        float tileH = map.tileheight;

        foreach (var layer in map.layers)
        {
            if (layer.type != "objectgroup" || layer.objects == null) continue;

            foreach (var obj in layer.objects)
            {
                if (obj.type != "Collider" || obj.width <= 0 || obj.height <= 0) continue;

                // 1. Tọa độ Y Tiled (pixel, gốc trên)
                float tiledYTop = obj.y;
                float tiledYBottom = obj.y + obj.height;

                // 2. Chuyển đổi tọa độ Tiled Y sang World Y (Logic Y tăng từ dưới lên)
                // World Y Top (tọa độ pixel thấp) -> Logic Y cao
                float worldYTop = (map.height * tileH) - tiledYTop;

                // World Y Bottom (tọa độ pixel cao) -> Logic Y thấp
                float worldYBottom = (map.height * tileH) - tiledYBottom;

              
                int tempMinTileY = Mathf.Max(0, Mathf.FloorToInt(worldYBottom / tileH));

                // Lấy chỉ số logic Y cao nhất (max Tile Y index)
                // Math.FloorToInt(worldYTop / tileH) là logic Y index cao.
                // Nếu Object nằm hoàn toàn trong ô đó, chúng ta muốn lấy chỉ số ngay trước biên (floor)
                int tempMaxTileY = Mathf.Min(gridHeight - 1, Mathf.FloorToInt(worldYTop / tileH) - 1);

                int maxTileY = Mathf.Min(gridHeight - 1, Mathf.CeilToInt(tiledYBottom / tileH) - 1);

                // Lấy chỉ số Y cao (gần đỉnh map)
                // obj.y là tọa độ pixel của đỉnh Object.
                int minTileY = Mathf.Max(0, Mathf.FloorToInt(tiledYTop / tileH));

                // Lấy chỉ số X (giữ nguyên)
                int minTileX = Mathf.Max(0, Mathf.FloorToInt(obj.x / tileW));
                int maxTileX = Mathf.Min(gridWidth - 1, Mathf.CeilToInt((obj.x + obj.width) / tileW) - 1);


                // --- LẶP QUA CÁC Ô BỊ CHẶN ---

                // Duyệt theo Tiled Y logic (tăng từ trên xuống)
                for (int y = minTileY; y <= maxTileY; y++) // y ở đây là Tiled Y index (tăng từ trên xuống)
                {
                    for (int x = minTileX; x <= maxTileX; x++)
                    {
                        var node = gridNodes[y, x];
                        if (node == null) continue;
                        node.walkable = false;
                    }
                }

            }
        }
    }
    private void OnDrawGizmos()
    {
        if(!drawGizmo||!Application.isPlaying) return;
        TileNode[,] gridNodes = pathfinder.grid;
        // Chỉ vẽ Gizmos khi game đang chạy và gridNodes đã được khởi tạo
        if (gridNodes == null || !Application.isPlaying)
            return;

        int gridHeight = gridNodes.GetLength(0);
        int gridWidth = gridNodes.GetLength(1);

        // Lặp qua tất cả các Node
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                TileNode node = gridNodes[y, x];
                if (node == null) continue;

                // 1. Xác định màu
                if (node.walkable == false)
                {
                    // Màu đỏ cho các ô bị chặn/Collider chặn
                    Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
                }
                else
                {
                    // Màu xanh lá nhạt cho các ô có thể đi
                    Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
                }

                // 2. Lấy vị trí World của ô (tâm)
                // Vì TileNode lưu vị trí góc (từ CellToWorld), ta cần cộng 1/2 kích thước ô
                // **LƯU Ý:** Bạn cần truy cập kích thước ô (ví dụ: map.tilewidth/tileheight) 
                // Nếu kích thước ô là 1 World Unit, thì WorldPos đã đúng.

                // Nếu bạn dùng PPU=48 và 1 tile = 1 World Unit: tileWidthWorld = 1.
                // Nếu bạn dùng PPU=1 và 1 tile = 48 World Units: tileWidthWorld = 48.

                // Dùng kích thước Tiled Object để đồng bộ hóa
                float worldTileWidth = map.tilewidth;
                float worldTileHeight = map.tileheight;

                // Tính vị trí tâm của ô: Góc + 1/2 kích thước
                Vector3 center = node.worldPos;
                center.x += worldTileWidth / 2f;
                center.y += worldTileHeight / 2f;

                // 3. Vẽ hình hộp 2D (Cube) tại vị trí tâm
                Gizmos.DrawCube(center, new Vector3(worldTileWidth, worldTileHeight, 0.1f));
            }
        }
    }
}
