using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{

    private static MonsterManager instance;
    public static MonsterManager GetInstance() => instance;

    List<GameObject> monsters= new List<GameObject>();

    private void Awake()
    {
        if ( instance == null)
        {
            instance = this;
        }
    }

    public void AddMonster(GameObject _monster)
    {
        monsters.Add(_monster);
    }

    public bool IsLastMonster(GameObject _monster)
    {
        return monsters.Count - 1 == monsters.IndexOf(_monster);
    }
}
