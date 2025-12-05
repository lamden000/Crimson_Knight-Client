using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private GameObject CharacterPrefab;

    [SerializeField]
    private GameObject OtherCharacterPrefab;

    [SerializeField]
    private GameObject monsterPrefab;


    private void Awake()
    {
        Instance = this;
    }

    public static SpawnManager Instance;
    public static SpawnManager GI()
    {
        return Instance;
    }


    public  GameObject SpawnCharacter(int x, int y)
    {
        if (CharacterPrefab == null)
        {
            Debug.LogError("Lỗi: CharacterPrefab chưa được gán.");
            return null;
        }
        return Instantiate(CharacterPrefab, new Vector3(x, y, 0), Quaternion.identity);
    }

    public GameObject SpawnOtherCharacter(int x, int y)
    {
        if (OtherCharacterPrefab == null)
        {
            Debug.LogError("Lỗi: CharacterPrefab chưa được gán.");
            return null;
        }
        return Instantiate(OtherCharacterPrefab, new Vector3(x, y, 0), Quaternion.identity);
    }


    public GameObject SpawnMonster(int x, int y)
    {
        return null;
    }
}
