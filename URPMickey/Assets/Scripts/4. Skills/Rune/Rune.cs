using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rune : MonoBehaviour
{
    [Header("기본 컴포넌트")]
    [SerializeField] private GameObject onDestroyVFXPrefab;

    public RuneData Data { get; private set; }

    void OnEnable()
    {

    }

    void OnDisable()
    {

    }

    public void OnSelected()
    {
        PopupBase popup = PopupManager.Instance.ShowPopup(PopupType.Rune);

        RuneDescriptionPopup runePopup = popup as RuneDescriptionPopup;

        if (runePopup != null)
        {
            runePopup.Setup(Data);
        }
    }

    public void OnDropped()
    {
        PopupManager.Instance.ClosePopup(PopupType.Rune);
    }
    
    public void SetRuneData(RuneData _data)
    {
        Data = _data;
    }
}
