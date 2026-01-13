using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private GameObject CharacterPrefab;


    [SerializeField]
    private GameObject monsterPrefab;

    [SerializeField]
    private GameObject npcPrefab;

    [SerializeField]
    private GameObject displayBaseObjectNamePrefab;

    [SerializeField]
    private GameObject txtDisplayTakeDamagePrefab;

    [SerializeField]
    private GameObject pkIconPrefab;

    [SerializeField]
    private GameObject skillPrefab;

    private void Awake()
    {
        Instance = this;
        ResourceManager.Load();
    }

    public static SpawnManager Instance;


    public static SpawnManager GI()
    {
        return Instance;
    }
    public GameObject SpawnCharacterPrefab(int x, int y)
    {
        if (CharacterPrefab == null)
        {
            Debug.LogError("Lỗi: CharacterPrefab chưa được gán.");
            return null;
        }
        return Instantiate(CharacterPrefab, new Vector3(x, y, 0), Quaternion.identity);
    }


    public GameObject SpawnMonsterPrefab(int x, int y, int templateId)
    {
        if (monsterPrefab == null)
        {
            Debug.LogError("Monster prefab not assigned!");
            return null;
        }

        GameObject monster = Instantiate(monsterPrefab, new Vector2(x, y), Quaternion.identity);
        var monsterCtrl = monster.GetComponent<MonsterPrefab>();
        monsterCtrl.ImageId = TemplateManager.MonsterTemplates[templateId].ImageId;
        return monster;
    }

    public GameObject SpawnNpcPrefab(int x, int y, int templateId)
    {
        if (npcPrefab == null)
        {
            Debug.LogError("Npc prefab not assigned!");
            return null;
        }
        GameObject npc = Instantiate(npcPrefab, new Vector2(x, y), Quaternion.identity);
        var npcCtrl = npc.GetComponent<NPCPrefab>();
        if (npcCtrl != null)
        {
            npcCtrl.Init(TemplateManager.NpcTemplates[templateId]);
        }
        return npc;
    }

    public GameObject SpawnDisplayBaseObjectNamePrefab(string name)
    {
        if (displayBaseObjectNamePrefab == null)
        {
            Debug.LogError("DisplayBaseObjectName prefab not assigned!");
            return null;
        }
        GameObject displayObj = Instantiate(displayBaseObjectNamePrefab, Vector3.zero, Quaternion.identity);
        TextMeshPro tmp = displayObj.GetComponent<TextMeshPro>();
        if (tmp != null)
        {
            tmp.text = name;
        }
        else
        {
            Debug.LogError("TextMeshPro k tim thay!");
        }
        return displayObj;
    }

    public GameObject SpawnTxtDisplayTakeDamagePrefab(int x, int y, int dame)
    {
        IEnumerator MoveUpAndDestroy(GameObject obj, float lifeTime, float speed)
        {
            float timer = 0f;

            while (timer < lifeTime)
            {
                if (obj == null)
                    yield break;

                obj.transform.position += Vector3.up * speed * Time.deltaTime;
                timer += Time.deltaTime;
                yield return null;
            }

            Destroy(obj);
        }

        if (txtDisplayTakeDamagePrefab == null)
        {
            Debug.Log("TxtDisplayTakeDamagePrefab prefab not assigned!");
            return null;
        }

        GameObject obj = Instantiate(
            txtDisplayTakeDamagePrefab,
            new Vector2(x, y),
            Quaternion.identity
        );

        TextMeshPro tmp = obj.GetComponent<TextMeshPro>();
        if (tmp != null)
        {
            tmp.text = "-" + dame;
        }

        StartCoroutine(MoveUpAndDestroy(obj, 1.0f, 20.0f));

        return obj;
    }

    public GameObject SpawnPkIconPrefab()
    {
        if (pkIconPrefab == null)
        {
            Debug.LogError("pkIconPrefab prefab not assigned!");
            return null;
        }
        return Instantiate(pkIconPrefab, Vector3.zero, Quaternion.identity);
    }

    private static Dictionary<string, SkillSpawnData> spawnDatas = new Dictionary<string, SkillSpawnData>();

    public GameObject SpawnPickItem(int templateId, ItemType type, Vector2 pos)
    {

        GameObject itemObj = new GameObject($"Item_{templateId}_{type}");
        itemObj.transform.position = pos;

        int iconId = -1;
        Sprite sprite = null;
        SpriteRenderer sr = itemObj.AddComponent<SpriteRenderer>();

        if (templateId != -1)
        {
            switch (type)
            {
                case ItemType.Equipment:
                    iconId = TemplateManager.ItemEquipmentTemplates[templateId].IconId;
                    break;
                case ItemType.Consumable:
                    iconId = TemplateManager.ItemConsumableTemplates[templateId].IconId;
                    break;
                case ItemType.Material:
                    iconId = TemplateManager.ItemMaterialTemplates[templateId].IconId;
                    break;
            }
            if (iconId == -1)
            {
                Debug.LogError($"Không thể tạo Item với type: {type}");
                Destroy(itemObj);
                return null;
            }
            switch (type)
            {
                case ItemType.Equipment:
                    ResourceManager.ItemEquipmentIconSprites.TryGetValue(iconId, out sprite);
                    break;
                case ItemType.Consumable:
                    ResourceManager.ItemConsumableIconSprites.TryGetValue(iconId, out sprite);
                    break;
                case ItemType.Material:
                    ResourceManager.ItemMaterialsIconSprites.TryGetValue(iconId, out sprite);
                    break;
            }
        }
        else
        {
            sprite = ResourceManager.SpriteGold;
        }
        if (sprite == null)
        {
            Debug.LogWarning($"Không tìm thấy sprite Item iconId={iconId}, type={type}");
        }
        else
        {
            sr.sprite = sprite;
        }

        return itemObj;
    }

    public void SpawnEffectPrefab(string effectName, Transform caster, Transform target, float duration = 0.5f)
    {
        if (caster == null)
        {
            Debug.LogError("Caster không được null!");
            return;
        }

        if (!spawnDatas.TryGetValue(effectName, out SkillSpawnData spawnData))
        {
            string resourcePath = $"Skills/Data/Skills/{effectName}";
            spawnData = Resources.Load<SkillSpawnData>(resourcePath);
            if (spawnData == null)
            {
                Debug.LogError($"Không tìm thấy SkillSpawnData: {effectName} tại {resourcePath}");
                return;
            }
            spawnDatas.TryAdd(effectName, spawnData);
        }

        if (skillPrefab == null)
        {
            Debug.LogError("Không tìm thấy Skill Prefab!");
            return;
        }

        GameObject skillInstance = Instantiate(
            skillPrefab,
            caster.position,
            Quaternion.identity
        );

        var skillSpawner = skillInstance.GetComponent<SkillSpawnmer>();

        if (skillSpawner != null)
        {
            skillSpawner.Init(
                spawnData,
                caster.position,
                target != null ? target.position : caster.position,
                target,
                null, // spawnPositions - giữ null vì không có từ warning ở đây
                duration // truyền duration vào
            );
        }
        else
        {
            Debug.LogError("SkillSpawnmer component không tìm thấy!");
            Destroy(skillInstance);
        }
    }

}
