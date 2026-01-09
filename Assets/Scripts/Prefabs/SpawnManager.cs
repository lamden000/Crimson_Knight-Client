using System.Collections;
using TMPro;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private GameObject CharacterPrefab;

    [SerializeField]
    private GameObject OtherCharacterPrefab;

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


    private void Awake()
    {
        Instance = this;
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

    public GameObject SpawnOtherCharacterPrefab(int x, int y)
    {
        if (OtherCharacterPrefab == null)
        {
            Debug.LogError("Lỗi: CharacterPrefab chưa được gán.");
            return null;
        }
        return Instantiate(OtherCharacterPrefab, new Vector3(x, y, 0), Quaternion.identity);
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

}
