using UnityEngine;

public class DamageAnimations : MonoBehaviour
{
    [SerializeField] private DamageTextBase damageObj;

    public void ReturnToPool()
    {
        damageObj.ReturnToPool();    
    }
}
