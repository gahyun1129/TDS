using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class StartInfoPopUp : PopupBase
{
    [SerializeField] Animator animator;
    [SerializeField] Button playBtn;

    void Start()
    {
        playBtn.onClick.AddListener(StartGame);
    }

    public void StartGame()
    {
        if (InGameManager.Instance != null)
        {
            InGameManager.Instance.GameStart();
        }
        
        // canMoving은 BearController.OnGameStart()에서 이미 처리되므로 중복 제거
        // InGameManager.GameStart() -> OnGameStart 이벤트 -> BearController.OnGameStart() -> canMoving = true
        
        animator.SetTrigger("Hide");
    }
}
