using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RuneDescriptionPopup : PopupBase
{
    [Header("룬 상세창 팝업")]
    [SerializeField] private Text runeNameText;
    [SerializeField] private Text runeDescriptionText;
    [SerializeField] private Image runeIconImage;

    public void Setup(RuneData data)
    {
        runeNameText.text = data.runeName;
        runeDescriptionText.text = data.description;
        runeIconImage.sprite = data.runeIcon;
    }
}
