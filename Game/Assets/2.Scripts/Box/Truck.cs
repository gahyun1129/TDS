using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Truck : MonoBehaviour
{

    private static Truck instance;
    public static Truck GetInstance() => instance;

    [SerializeField] List<Box> boxes = new List<Box>();
    [SerializeField] GameObject text;


    private void Awake()
    {
        if ( instance == null)
        {
            instance = this;
        }
    }

    public void OnDamaged(int damage)
    {
        if ( boxes.Count > 0)
        {
            boxes[0].OnDamaged(damage);

            if (boxes[0].IsBroken())
            {
                Box _box = boxes[0];
                boxes.RemoveAt(0);

                Destroy(_box.gameObject);

                if ( boxes.Count == 0 )
                {
                    
                    MonsterManager.GetInstance().MonsterWin();
                    text.SetActive(true);
                    gameObject.SetActive(false);
                }
            }
        }
    }

}
