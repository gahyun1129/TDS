using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InvenSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("룬 정보")]
    [SerializeField] private Image icon;
    [SerializeField] private RuneData equippedRune;
    [SerializeField] private DraggableInvenItem invenRune;

    public void EquippedRune(GameObject _rune)
    {
        equippedRune = _rune.GetComponent<Rune>().Data;
        icon.sprite = equippedRune.runeIcon;
        invenRune.rune = equippedRune;
        Debug.Log("룬 줄게");
    }

    public void UnequippedRune()
    {
        equippedRune = null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if ( equippedRune != null)
        {
            PopupBase popup = PopupManager.Instance.ShowPopup(PopupType.Rune);

            RuneDescriptionPopup runePopup = popup as RuneDescriptionPopup;

            if (runePopup != null)
            {
                runePopup.Setup(equippedRune);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PopupManager.Instance.ClosePopup(PopupType.Rune);
    }
}
