using UnityEngine;

public class OutGameManager : MonoBehaviour
{
    public static OutGameManager Instacne { get; private set; }

    [Header("아웃 게임 UI")]
    [SerializeField] private OutGameUIInterect UI;

    void Awake()
    {
        if (Instacne == null)
        {
            Instacne = this;
        }
    }


}
