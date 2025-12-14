using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearPopUpAnimation : MonoBehaviour
{
    [SerializeField] GameSuccessPopUp gameSuccessPopUp;

    public void OnEndShow()
    {
        gameSuccessPopUp.PlayEffect();
    }
}
