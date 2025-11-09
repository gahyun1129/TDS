using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("스킬 슬롯 정보")]
    [SerializeField] private int index;
    [SerializeField] private SkillData equippedSkill;

    SkillManager skillManager;


    void Start()
    {
        skillManager = BattleManager.Instance.Player.GetComponent<SkillManager>();
        if ( skillManager.equippedSkills.Count > index)
        {
            equippedSkill = skillManager.equippedSkills[index];
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PopupBase popup = PopupManager.Instance.ShowPopup(PopupType.Skill);

        SkillPopup skillPopup = popup as SkillPopup;

        skillPopup.Setup(equippedSkill);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PopupManager.Instance.ClosePopup(PopupType.Skill);
    }

}
