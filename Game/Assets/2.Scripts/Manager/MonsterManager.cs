using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{

    private static MonsterManager instance;
    public static MonsterManager GetInstance() => instance;

    [SerializeField] List<List<Monster>> monsters= new List<List<Monster>>();

    private void Awake()
    {
        if ( instance == null)
        {
            instance = this;
        }
        monsters.Add(new List<Monster>());
    }

    private void Start()
    {
        StartCoroutine(ManageMonster());
    }

    IEnumerator ManageMonster()
    {
        while (true)
        {
            for(int i = 1; i < monsters.Count; i++)
            {
                if (monsters[i].Count > 0 && !monsters[i][0].GetIsMoving())
                {
                    MoveMonstersToLeft(i);
                    MoveMonstersToRight(i - 1);

                    Monster _monster = monsters[i][0];
                    monsters[i].Remove(_monster);
                    monsters[i - 1].Insert(0, _monster);
                }
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator JumpMonster(int _layer, Monster _monster)
    {

        while(true)
        {
            if ( !_monster.GetIsMoving())
            {
                break;
            }

            yield return null;
        }

        _monster.Jump();

        yield return new WaitForSeconds(0.3f);

        if ( monsters.Count - 1 < (_layer + 1) )
        {
            monsters.Add(new List<Monster>());
        }

        monsters[_layer].Remove(_monster);
        AddMonster(_layer + 1, _monster);
        //monsters[_layer + 1].Add(_monster);

        yield return new WaitForSeconds(0.3f);
    }

    public void AddMonster(int _layer, Monster _monster)
    {
        monsters[_layer].Add(_monster);

        if (monsters[_layer].Count > 2)
        {
            StartCoroutine(JumpMonster(_layer, _monster));
        }
    }

    public void MoveMonstersToRight(int _layer)
    {
        foreach(Monster _monster in monsters[_layer])
        {
            _monster.MoveRightDirection(2f);
        }
    }
    public void MoveMonstersToLeft(int _layer)
    {
        foreach (Monster _monster in monsters[_layer])
        {
            _monster.MoveRightDirection(2f);
        }
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
}
