using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    [Header("적 오브젝트 풀링")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private List<GameObject>[] enemyPools;

    [Header("파편 오브젝트 풀링")]
    [SerializeField] private GameObject cogPrefab;
    [SerializeField] private List<GameObject> cogPool;

    [Header("데미지 UI 오브젝트 풀링")]
    [SerializeField] private GameObject damagePrefab;
    [SerializeField] private List<GameObject> damagePool;
    [SerializeField] private int initialPoolSize;

    [Header("풀 담을 오브젝트")]
    [SerializeField] private GameObject enemyPoolObj;
    [SerializeField] private GameObject runePoolObj;
    [SerializeField] private GameObject DamageUIPoolObj;

    public static ObjectPoolManager Instance { get; private set; }


    void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
        }
        enemyPools = new List<GameObject>[enemyPrefabs.Length];

        for (int index = 0; index < enemyPools.Length; ++index)
        {
            enemyPools[index] = new List<GameObject>();
        }

        cogPool = new List<GameObject>();

        damagePool = new List<GameObject>();
        InitializePool();
    }


    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject obj = Instantiate(damagePrefab, DamageUIPoolObj.transform);
            GameObject damageText = obj;
            obj.SetActive(false); // 비활성화 상태로 풀에 넣음
            damagePool.Add(damageText);
        }
    }

    public int GetEnemyPrefabCount => enemyPrefabs.Length;

    public GameObject GetEnemy(int index)
    {
        GameObject enemy = null;

        foreach (GameObject item in enemyPools[index])
        {
            if (!item.activeSelf)
            {
                enemy = item;
                enemy.SetActive(true);
                break;
            }
        }

        if (!enemy)
        {
            enemy = Instantiate(enemyPrefabs[index], enemyPoolObj.transform);
            enemy.name = "Enemy A - " + enemyPools[index].Count;
            enemyPools[index].Add(enemy);
        }

        return enemy;
    }

    public void ReturnToPool(GameObject deadObject)
    {
        deadObject.SetActive(false);
    }

    public GameObject GetCog()
    {
        GameObject cog = null;

        foreach (GameObject item in cogPool)
        {
            if (!item.activeSelf)
            {
                cog = item;
                cog.SetActive(true);
                break;
            }
        }

        if (!cog)
        {
            cog = Instantiate(cogPrefab, runePoolObj.transform);
            cogPool.Add(cog);
        }

        return cog;
    }

    public GameObject GetDamage()
    {
        GameObject damage = null;

        foreach (GameObject dmg in damagePool)
        {
            if (!dmg.activeSelf)
            {
                damage = dmg;
                damage.SetActive(true);
                break;
            }
        }

        if (!damage)
        {
            damage = Instantiate(damagePrefab, DamageUIPoolObj.transform);
            damagePool.Add(damage);
        }

        return damage;
    }

    public void ShowDamage(string text, Vector3 worldPosition)
    {
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        GameObject damageText = GetDamage();
        
        damageText.gameObject.SetActive(true);

        damageText.GetComponent<DamageText>().ShowText(text, screenPosition);
    }
}
