using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawn : MonoBehaviour
{
    [SerializeField] float coolTime = 3f;
    [SerializeField] List<GameObject> monsterPrefeb = new List<GameObject>();

    private void Start()
    {
        StartCoroutine(SpawnMonster());
    }

    IEnumerator SpawnMonster()
    {
        while (true)
        {
            int _id = Random.Range(0, monsterPrefeb.Count);

            if (monsterPrefeb[_id] != null)
            {
                GameObject _monster = Instantiate(monsterPrefeb[_id], transform.position, Quaternion.identity);

                MonsterManager.GetInstance().AddMonster(0, _monster.GetComponent<Monster>());
            }
            yield return new WaitForSeconds(coolTime);
        }
    }
}
