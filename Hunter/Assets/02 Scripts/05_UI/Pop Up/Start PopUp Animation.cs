using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class StartPopUpAnimation : MonoBehaviour
{
    [SerializeField] Transform birdStartTrans, enemyStartTrnas;
    [SerializeField] Transform birdTarget, enemyTarget;
    [SerializeField] Transform birdIcon, enemyIcon;
    [SerializeField] float iconScale = 1.5f;
    [SerializeField] float moveDuation = 0.5f;
    [SerializeField] float sclaeUpDuration = 0.1f;
    [SerializeField] float scaleEndDuration = 0.5f;
    [SerializeField] float moveDelay = 0.2f;
    [SerializeField] float scaleDelay = 0.2f;
    [SerializeField] float InGameShowTime = 1f;

    public void MoveIcon()
    {
        birdIcon.gameObject.SetActive(true);
        enemyIcon.gameObject.SetActive(true);

        birdIcon.position = birdStartTrans.position;
        enemyStartTrnas.position = enemyStartTrnas.position;

        birdIcon.DOKill();
        enemyIcon.DOKill();

        birdIcon.DOScale(iconScale, sclaeUpDuration);
        enemyIcon.DOScale(iconScale, sclaeUpDuration);

        birdIcon.DOMove(birdTarget.position, moveDuation).SetDelay(moveDelay);
        enemyIcon.DOMove(enemyTarget.position, moveDuation).SetDelay(moveDelay);

        scaleDelay += sclaeUpDuration;

        birdIcon.DOScale(0f, scaleEndDuration).SetDelay(scaleDelay).OnComplete(() => birdIcon.gameObject.SetActive(false));
        enemyIcon.DOScale(0f, scaleEndDuration).SetDelay(scaleDelay).OnComplete(() => enemyIcon.gameObject.SetActive(false));

        Invoke("ShowInGameUI", InGameShowTime);
    }

    public void ClosePopup()
    {
        PopupManager.Instance.ClosePopup(PopupType.StartInfo);
    }
}
