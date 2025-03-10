using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawn : MonoBehaviour
{
    [SerializeField] float coolTime = 3f;
    [SerializeField] List<GameObject> monsterPrefeb = new List<GameObject>();

    int layer = 0;
    bool isMonsterWin = false;

    private void Start()
    {
        StartCoroutine(SpawnMonster());
    }

    IEnumerator SpawnMonster()
    {
        while (!isMonsterWin)
        {
            int _id = Random.Range(0, monsterPrefeb.Count);

            if (monsterPrefeb[_id] != null)
            {
                GameObject _monster = Instantiate(monsterPrefeb[_id], new Vector3(transform.position.x, transform.position.y + layer * 1.2f, transform.position.z), Quaternion.identity);
                _monster.GetComponent<Monster>().SetLayer(layer);

                MonsterManager.GetInstance().AddMonster(layer, _monster.GetComponent<Monster>());
            }
            yield return new WaitForSeconds(coolTime);
        }
    }

    public void SetLayer(int _layer) => layer = _layer;
    public void SetMonsterWin(bool _isMonsterWin) => isMonsterWin = _isMonsterWin;
}
