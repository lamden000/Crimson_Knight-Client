using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterSpriteDatabase : MonoBehaviour
{

    enum EnemySprite { Idle_1 = 0, Walk_1 = 1, Walk_2 = 2, Attack = 3, GetHit = 4, Idle_2 = 5 }

    private Dictionary<EnemyName,Dictionary<EnemyState,List<Sprite>>> database
      = new Dictionary<EnemyName, Dictionary<EnemyState, List<Sprite>>>();

    public string folderPath = "Enemies";
    public static MonsterSpriteDatabase Instance { get; private set; }

    private List<EnemyName> loadedEnemy = new List<EnemyName>();
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    public void LoadSprites(EnemyName enemyName)
    {
        if (loadedEnemy.Contains(enemyName))
            return;

        AddSprite(enemyName, EnemySprite.Walk_1, EnemyState.Walk);
        AddSprite(enemyName, EnemySprite.Walk_2, EnemyState.Walk);

        AddSprite(enemyName, EnemySprite.Attack, EnemyState.Attack);
        AddSprite(enemyName, EnemySprite.GetHit, EnemyState.GetHit);

        AddSprite(enemyName, EnemySprite.Idle_1, EnemyState.Idle);
      //  AddSprite(enemyName, EnemySprite.Idle_2, EnemyState.Idle);

        loadedEnemy.Add(enemyName);
    }

    private void AddSprite(EnemyName enemyName,EnemySprite stateCode, EnemyState state)
    {
        if (!database.ContainsKey(enemyName))
            database[enemyName] = new Dictionary<EnemyState, List<Sprite>>();

        if (!database[enemyName].ContainsKey(state))
            database[enemyName][state] = new List<Sprite>();

        int enemyIndex = (int)enemyName;
        string sheetName = $"{enemyIndex}";
        string spriteName = $"{enemyIndex}_{(int)stateCode}";

        Sprite[] all = Resources.LoadAll<Sprite>(folderPath + "/" + sheetName);
        Sprite sprite = System.Array.Find(all, s => s.name == spriteName);

        if (sprite != null)
            database[enemyName][state].Add(sprite);
        else if((int)stateCode!=5)
            Debug.LogWarning($"Sprite not found: {spriteName}");
    }

    public List<Sprite> GetSprites(EnemyName name,EnemyState state)
    {
        if (database.TryGetValue(name, out var enemyDict))
        {
            if (enemyDict.TryGetValue(state, out var spriteList))
            {
                return spriteList;
            }
        }
        return null;
    }

}
