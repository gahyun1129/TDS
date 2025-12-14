using UnityEngine;
using UnityEngine.UI;

public class ResultUITest : MonoBehaviour
{
    [SerializeField] Button btnPlay;


    void Start()
    {
        btnPlay.onClick.AddListener(GetBtnPlay);
    }

    void GetBtnPlay()
    {

    }

    public void GoodsMove()
    {
        // 재화 날아가는 연출
    }
}
