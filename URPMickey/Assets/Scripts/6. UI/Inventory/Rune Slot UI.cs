using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // 이벤트 시스템 사용을 위해 필수

public class RuneSlotUI : MonoBehaviour, IDropHandler
{
    [Header("룬 슬롯 정보")]
    public SkillData skill;
    [SerializeField] private Image runeIcon;
    [SerializeField] private RuneData equippedRune;

    // 이 슬롯에 아이템이 드롭되었을 때 호출됨
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log(gameObject.name + "에 OnDrop!");

        // 1. 드래그 중이던 게임 오브젝트를 가져옴
        GameObject droppedObject = eventData.pointerDrag;

        // 2. 드래그 중이던 오브젝트에서 DraggableItem 스크립트를 가져옴
        DraggableInvenItem item = droppedObject.GetComponent<DraggableInvenItem>();

        if (item != null)
        {
            // 3. (핵심) 이 아이템이 돌아갈 부모(parentToReturnTo)를
            // "자기 자신(이 슬롯의 transform)"으로 설정
            // 이렇게 하면 DraggableItem의 OnEndDrag가 실행될 때,
            // 이 슬롯을 부모로 삼게 됨
            skill.EuippedRune(item.rune);
            equippedRune = item.rune;
            runeIcon.sprite = equippedRune.icon;
            item.runeSlot = null;
            item.rune = null;
        }
    }
}