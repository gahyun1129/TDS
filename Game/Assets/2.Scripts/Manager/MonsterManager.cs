using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{

    private static MonsterManager instance;
    public static MonsterManager GetInstance() => instance;

    List<List<GameObject>> monsters= new List<List<GameObject>>();

    private void Awake()
    {
        if ( instance == null)
        {
            instance = this;
        }
        monsters.Add(new List<GameObject>());
    }

    public void AddMonster(int _layer, GameObject _monster)
    {
        monsters[_layer].Add(_monster);
    }

    public bool IsLastMonster(int _layer, GameObject _monster)
    {
        return monsters[_layer].Count - 1 == monsters[_layer].IndexOf(_monster) && monsters[_layer].Count != 1;
    }

    public void MoveMonsterToNextLayer(int _prevLayer, int _nextLayer, GameObject _monster)
    {
        if ( monsters.Count - 1 < _nextLayer )
        {
            monsters.Add(new List<GameObject>());
        }
        monsters[_prevLayer].Remove(_monster);
        monsters[_nextLayer].Add(_monster);
    }

    public void MoveMonsterToPrevLayer(int _curLayer, int _prevLayer, GameObject _monster)
    {
        monsters[_curLayer].Remove(_monster);
        monsters[_prevLayer].Insert(0, _monster);
    }

    public bool IsFirstMonster(int _layer, GameObject _monster)
    {
        return monsters[_layer].IndexOf(_monster) == 0;
    }

    public void MoveMonsters(int _layer)
    {
        foreach(GameObject _monster in monsters[_layer])
        {
            _monster.GetComponent<Monster>().MoveRightDirection();
        }
    }
}
