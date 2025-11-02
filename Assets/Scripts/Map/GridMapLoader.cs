using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEditor;
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
    public GameObject monsterPrefab;
    public GameObject npcPrefab;
    public int subGridDivisions=2;

    private Dictionary<int, Tile> gidToTile = new Dictionary<int, Tile>();
    public bool loadInEditMode = false;
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

            foreach (var ts in map.tilesets)
                LoadTileset(ts, usedGids);

            LoadTileLayers();

            LoadObjectLayers();
        }
    }

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

            // 🔸 Nếu là folder "Objects" thì load sprite sheet multiple
            Sprite[] sprites;
            Sprite s = Resources.Load<Sprite>(resourcePath);
            if (s == null)
            {
                continue;
            }
            sprites = new Sprite[] { s };

            if (!gidToTile.ContainsKey(gid))
            {
                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sprites[0];
                gidToTile[gid] = tile;
                loadedCount++;
            }
        }

       // Debug.Log($"✅ Loaded {loadedCount} tiles from '{folder}'");
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

        // --- B2: tạo subgrid chi tiết từ collider thực tế ---
        TileNode[,] subGrid = CreateSubGridFromColliders(gridNodes, subGridDivisions);

        // --- B3: gửi subgrid cho Pathfinder ---
        Pathfinder.Instance.Init(subGrid);

        AdjustBoundaryCollider();
    }

    TileNode[,] CreateSubGridFromColliders(TileNode[,] baseGrid, int subDivisions)
    {
        int baseHeight = baseGrid.GetLength(0);
        int baseWidth = baseGrid.GetLength(1);
        float tileW = map.tilewidth;
        float tileH = map.tileheight;

        // Subgrid có kích thước gấp subDivisions lần
        int newWidth = baseWidth * subDivisions;
        int newHeight = baseHeight * subDivisions;
        float subW = tileW / subDivisions;
        float subH = tileH / subDivisions;

        TileNode[,] subGrid = new TileNode[newHeight, newWidth];

        for (int y = 0; y < newHeight; y++)
        {
            for (int x = 0; x < newWidth; x++)
            {
                int baseX = x / subDivisions;
                int baseY = y / subDivisions;

                TileNode parent = baseGrid[baseY, baseX];
                if (parent == null) continue;

                float worldX = x * subW + subW / 2f;
                float worldY = y * subH + subH / 2f;
                Vector3 worldPos = new Vector3(worldX, worldY, 0f);

                bool walkable = true;

                foreach (var layer in map.layers)
                {
                    if (layer.type != "objectgroup" || layer.objects == null)
                        continue;

                    foreach (var obj in layer.objects)
                    {
                        if (obj.type != "Collider" || obj.width <= 0 || obj.height <= 0)
                            continue;

                        // --- Convert Tiled → Unity ---
                        float worldYBottom = (map.height * tileH) - (obj.y + obj.height);
                        float worldYTop = (map.height * tileH) - obj.y;

                        // --- AABB của subcell (có offset trừ) ---
                        float subMinX = (x * subW);
                        float subMaxX = subMinX + subW;
                        float subMinY = (y * subH);
                        float subMaxY = subMinY + subH;

                        // --- Overlap ---
                        float overlapX = Mathf.Max(0, Mathf.Min(subMaxX, obj.x + obj.width) - Mathf.Max(subMinX, obj.x));
                        float overlapY = Mathf.Max(0, Mathf.Min(subMaxY, worldYTop) - Mathf.Max(subMinY, worldYBottom));

                        float overlapArea = overlapX * overlapY;
                        float subArea = subW * subH;
                        float overlapRatio = overlapArea / subArea;

                        if (overlapRatio >= 0.3f)
                        {
                            walkable = false;
                            goto SkipRemaining;
                        }
                    }
                }

            SkipRemaining:
                subGrid[y, x] = new TileNode(x, y, worldPos, walkable);
            }
        }
        return subGrid;
    }


    private void AdjustBoundaryCollider()
    {
        if (map == null) return;

        float mapWorldWidth = map.width * map.tilewidth;
        float mapWorldHeight = map.height * map.tileheight;

        // Find candidate child colliders (exclude colliders created for objects)
        var colliders = GetComponentsInChildren<BoxCollider2D>(true);
        BoxCollider2D boundary = null;

        foreach (var c in colliders)
        {
            if (c.gameObject == this.gameObject) continue;
            string n = c.gameObject.name;
            // exclude runtime object colliders we create for tiles/objects
            if (n.StartsWith("ColliderBox") || n.StartsWith("WaterBox")) continue;
            // prefer explicit names containing "Bound" or "Map"
            if (n.IndexOf("Bound", StringComparison.OrdinalIgnoreCase) >= 0 || n.IndexOf("Map", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                boundary = c;
                break;
            }
        }

        // fallback: pick the first child collider that isn't one of the object colliders
        if (boundary == null)
        {
            foreach (var c in colliders)
            {
                if (c.gameObject == this.gameObject) continue;
                string n = c.gameObject.name;
                if (n.StartsWith("ColliderBox") || n.StartsWith("WaterBox")) continue;
                boundary = c;
                break;
            }
        }

        // If still not found, create a new child specifically for boundary
        if (boundary == null)
        {
            var go = new GameObject("MapBoundary");
            go.transform.SetParent(transform, false);
            boundary = go.AddComponent<BoxCollider2D>();
            boundary.isTrigger = true;
            boundary.offset = Vector2.zero;
        }
        boundary.size = new Vector2(mapWorldWidth, mapWorldHeight);

        boundary.transform.localPosition = new Vector3(mapWorldWidth / 2f, mapWorldHeight / 2f, 0f);
    }


    void LoadObjectLayers()
    {
        foreach (var layer in map.layers)
        {
            if (layer.type != "objectgroup" || layer.objects == null)
                continue;

            foreach (var obj in layer.objects)
            {
                switch (obj.type)
                {
                    case "Collider":
                        CreateColliderBox(obj, false);
                        break;

                    case "Water":
                        CreateColliderBox(obj, true);
                        break;

                    case "NPC":
                        SpawnNPC(obj);
                        break;

                    case "Monster":
                        SpawnMonster(obj);
                        break;

                    default:
                        if (obj.gid > 0 && gidToTile.TryGetValue(obj.gid, out Tile baseTile))
                            CreateStaticObject(obj, baseTile);
                        break;
                }
            }
        }
    }

    private void SpawnNPC(TiledObject obj)
    {
        if (npcPrefab == null)
        {
            Debug.LogError("NPC prefab not assigned!");
            return;
        }

        Vector3 pos = GetWorldPosition(obj);
        GameObject npc = Instantiate(npcPrefab, pos, Quaternion.identity);
        npc.name = $"NPC_{obj.name}_{obj.id}";

        // Auto-assign NPCName enum if it matches
        var npcCtrl = npc.GetComponent<NPC>();
        if (npcCtrl != null && System.Enum.TryParse(obj.name, out NPCName npcEnum))
        {
            npcCtrl.Init(npcEnum);
        }
    }

    private void SpawnMonster(TiledObject obj)
    {
        if (monsterPrefab == null)
        {
            Debug.LogError("Monster prefab not assigned!");
            return;
        }

        Vector3 pos = GetWorldPosition(obj);
        GameObject monster = Instantiate(monsterPrefab, pos, Quaternion.identity);
        monster.name = $"Monster_{obj.name}_{obj.id}";
        var monsterCtrl = monster.GetComponent<Monster>();
        if (monsterCtrl != null && System.Enum.TryParse(obj.name, out MonsterName npcEnum))
        {
            monsterCtrl.monsterName = npcEnum;
        }
    }

    private void CreateStaticObject(TiledObject obj, Tile baseTile)
    {
        GameObject parent = new GameObject($"Object_{obj.id}");
        float mapHeightInWorldUnits = map.height * map.tileheight;
        float centerX = obj.x + obj.width / 2f;
        float centerY = (mapHeightInWorldUnits - obj.y) - obj.height / 2f;

        var sr = parent.AddComponent<SpriteRenderer>();
        sr.sprite = baseTile.sprite;
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 0;
        sr.spriteSortPoint = SpriteSortPoint.Pivot;

        float spriteHeight = sr.bounds.size.y;

        Vector2 spriteSize = sr.sprite.rect.size;
        Vector2 pivotPixel = sr.sprite.pivot;

        float pixelsPerUnit = sr.sprite.pixelsPerUnit;
        Vector2 pivotDelta = (pivotPixel - spriteSize / 2f) / pixelsPerUnit;

        parent.transform.position = new Vector3(centerX + pivotDelta.x, centerY + spriteHeight + pivotDelta.y, 0);
    }

    private Vector3 GetWorldPosition(TiledObject obj)
    {
        float mapHeight = map.height * map.tileheight;
        float x = obj.x + obj.width / 2f;
        float y = (mapHeight - obj.y) - obj.height / 2f;
        return new Vector3(x, y, 0);
    }
void CreateColliderBox(TiledObject obj, bool isWater)
    {
        string name = isWater? "WaterBox":"ColliderBox";
        GameObject go = new GameObject(name);
        var col = go.AddComponent<BoxCollider2D>();

        col.isTrigger = isWater;

        float centerX = obj.x + obj.width / 2f;
        float mapHeightInWorldUnits = map.height * map.tileheight;
        float centerY = (mapHeightInWorldUnits - obj.y) - obj.height / 2f;

        go.transform.position = new Vector3(centerX, centerY, 0);

        col.size = new Vector2(obj.width, obj.height);

        col.offset = Vector2.zero;

        go.transform.SetParent(transform);
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmo||!Application.isPlaying) return;
        TileNode[,] gridNodes = pathfinder.grid;
        if (gridNodes == null)
            return;

        int gridHeight = gridNodes.GetLength(0);
        int gridWidth = gridNodes.GetLength(1);


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
