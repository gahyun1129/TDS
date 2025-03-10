using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class MonsterManager : MonoBehaviour
{

    private static MonsterManager instance;
    public static MonsterManager GetInstance() => instance;

    [SerializeField] List<List<Monster>> monsters= new List<List<Monster>>();
    [SerializeField] Vector3 truckPosition;
    [SerializeField] MonsterSpawn spawner;

    bool isJumping = false;
    Monster jumpingMonster = null;

    bool isRotating = false;
    Monster rotatingMonster = null;

    int layer = 0;

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
        StartCoroutine(JumpMonster(layer));
        StartCoroutine(RotateMonsters(layer + 1));
    }

    IEnumerator RotateMonsters(int _layer)
    {
        while (true)
        {
            if (!isRotating && monsters.Count > _layer && monsters[_layer].Count > 0)
            {
                if ( rotatingMonster == null)
                {
                    rotatingMonster = monsters[_layer][0];
                }
                if ( rotatingMonster != null && !rotatingMonster.GetIsMoving())
                {
                    RemoveMonster(_layer, rotatingMonster);

                    rotatingMonster.Fall(new Vector3(truckPosition.x, truckPosition.y + (_layer - 1) * 1.2f, 0));

                    isRotating = true;
                    
                    AddMonsterAtFirst(_layer - 1, rotatingMonster);
                    
                    ResetTarget(_layer - 1);
                }
            }
            else
            {
                if ( rotatingMonster != null && !rotatingMonster.GetIsMoving())
                {
                    isRotating = false;
                    rotatingMonster = null;

                    ResetTarget(_layer);

                    yield return new WaitForSeconds(1f);
                }
                if ( rotatingMonster == null)
                {
                    isRotating = false;
                    rotatingMonster = null;

                    if ( monsters.Count > _layer)
                    {
                        ResetTarget(_layer);
                    }

                    yield return new WaitForSeconds(1f);
                }
            }
            yield return null;
        }
    }

    public void ResetTarget(int _layer)
    {
        for (int i = 0; i < monsters[_layer].Count; ++i)
        {
            if (monsters[_layer][i] != rotatingMonster && monsters[_layer][i] != jumpingMonster)
            {
                monsters[_layer][i].SetTarget(new Vector3(truckPosition.x + i * 1.1f, truckPosition.y + (_layer) * 1.2f, 0));
            }
        }
    }

    IEnumerator JumpMonster(int _layer)
    {
        while (true)
        {
            if (!isJumping && monsters[_layer].Count > 1)
            {
                if (jumpingMonster == null)
                {
                    for( int i = 1; i < monsters[_layer].Count; ++i )
                    {
                        if (!monsters[_layer][i].GetIsMoving() && i + 4 >= monsters[_layer].Count)
                        {
                            jumpingMonster = monsters[_layer][i];
                        }
                    }
                }
                if (jumpingMonster != null && !jumpingMonster.GetIsMoving())
                {
                    RemoveMonster(_layer, jumpingMonster);
                    jumpingMonster.Jump(new Vector3(jumpingMonster.transform.position.x, truckPosition.y + (_layer + 1) * 1.2f, 0));

                    isJumping = true;
                }
            }
            else
            {
                if (jumpingMonster != null && !jumpingMonster.GetIsMoving())
                {
                    AddMonster(_layer + 1, jumpingMonster);

                    ResetTarget(_layer + 1);
                    isJumping = false;
                    jumpingMonster = null;

                    yield return new WaitForSeconds(2f);
                }
            }

            yield return null;
        }
    }

    public void AddMonster(int _layer, Monster _monster)
    {
        if ( monsters.Count - 1 < _layer )
        {
            monsters.Add(new List<Monster>());
        }

        _monster.SetTarget(new Vector3(truckPosition.x + monsters[_layer].Count * 1.1f, truckPosition.y + _layer * 1.2f, 0));
        monsters[_layer].Add(_monster);
    }

    public void AddMonsterAtFirst(int _layer, Monster _monster)
    {
        _monster.SetTarget(new Vector3(truckPosition.x, truckPosition.y + _layer * 1.2f, 0));
        monsters[_layer].Insert(0, _monster);
    }

    public void RemoveMonster(int _layer, Monster _monster)
    {
        monsters[_layer].Remove(_monster);

        ResetTarget(_layer);
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

    public void MonsterDie(int _layer, Monster _monster)
    {

        foreach(List<Monster> monsters in monsters)
        {
            if (monsters.Contains(_monster))
            {
                monsters.Remove(_monster);
            }
        }

        for (int i = 0; i < monsters.Count; ++i)
        {
            ResetTarget(i);
        }
    }

    public void MonsterWin()
    {
        foreach(List<Monster> monsters in monsters)
        {
            foreach(Monster _monster in monsters)
            {
                _monster.SetTarget(new Vector3(-20f, truckPosition.y, 0));
                Destroy(_monster.gameObject, 2f);
            }
        }
        spawner.SetMonsterWin(true);
    }
}
