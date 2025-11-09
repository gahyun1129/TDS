using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SkillPopup : PopupBase
{
    [Header("팝업창 컴포넌트")]
    [SerializeField] private Text skillNameText;
    [SerializeField] private Text skillDescriptionText;
    [SerializeField] private Image skillIcon;

    [SerializeField] private List<RuneSlotUI> runes;

    [SerializeField] private Button GoToNextButton;

    protected override void Awake()
    {
        base.Awake();

        skillNameText.text = "[]";
    }

    public void Setup(SkillData data)
    {
        skillNameText.text = data.finalSkillName;
        skillDescriptionText.text = data.findalDescription;
        skillIcon.sprite = data.finalIcon;

        foreach ( var slot in runes)
        {
            slot.skill = data;
        }
    }

    public void TurnOnNextButton(UnityAction onGoToNextStage)
    {
        GoToNextButton.gameObject.SetActive(true);
        GoToNextButton.onClick.AddListener(onGoToNextStage);
    }

    protected override void OnClosed()
    {
        GoToNextButton.gameObject.SetActive(false);
    }
}
