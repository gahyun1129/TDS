using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{

    private static MonsterManager instance;
    public static MonsterManager GetInstance() => instance;

    public List<List<Monster>> monsters= new List<List<Monster>>();

    private void Awake()
    {
        if ( instance == null)
        {
            instance = this;
        }
        monsters.Add(new List<Monster>());
    }

    public void ManageMonster(int _layer)
    {
        MoveMonsters(_layer);
        Monster _monster = monsters[_layer][0];
        monsters[_layer].Remove(_monster);
        monsters[_layer - 1].Insert(0, _monster);
    }

    void MoveMonsters(int _layer)
    {
        for ( int i = 0; i < monsters[_layer - 1].Count; ++i)
        {
            monsters[_layer - 1][i].MoveRightDirection(1f);
        }
    }

    public void AddMonster(int _layer, Monster _monster)
    {
        if ( monsters.Count <= _layer)
        {
            monsters.Add(new List<Monster>());
        }

        monsters[_layer].Add(_monster);
    }

    public bool IsMonsterInList()
    {
        return monsters[0].Count > 0;
    }
    public Monster GetFirstMonster()
    {
        if (monsters[0].Count > 0)
        {
            return monsters[0][0];
        }
        return null;
    }


    public void MonsterDie(Monster _monster)
    {
        foreach ( List<Monster> monsters in monsters)
        {
            if ( monsters.Contains(_monster))
            {
                monsters.Remove(_monster);
                break;
            }
        }
    }


    public bool AmILastMonsterInLayer(int _layer, Monster _monster)
    {
        if ( monsters.Count > _layer)
        {
            return monsters[_layer].Count > 1 && monsters[_layer].IndexOf(_monster) == monsters[_layer].Count - 1;
        }
        return false;
    }

    public bool AmIFirstMonsterInLayer(int _layer, Monster _monster)
    {
        return monsters.Count > _layer && _layer > 0 && monsters[_layer].IndexOf(_monster) == 0;
    }

    public void RemoveMonster(int _layer, Monster _monster)
    {
        monsters[_layer].Remove(_monster);
    }
}
