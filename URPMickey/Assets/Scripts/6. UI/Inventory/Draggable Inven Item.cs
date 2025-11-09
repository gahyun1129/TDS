using UnityEngine;
using UnityEngine.EventSystems; // 이벤트 시스템 사용을 위해 필수
using UnityEngine.UI; // Image 사용을 위해

[RequireComponent(typeof(CanvasGroup))] // 3. CanvasGroup을 필수로 요구
public class DraggableInvenItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector]
    public Transform parentToReturnTo = null; // 드롭 실패 시 돌아갈 부모(슬롯)

    private CanvasGroup canvasGroup;
    private Transform rootCanvas; // 최상위 캔버스

    private Vector3 originalScale;

    public RuneData rune;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>().transform;

        originalScale = GetComponent<RectTransform>().localScale;
    }

    // 1. 드래그가 시작됐을 때
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag");

        // 원래 부모(슬롯)를 기억
        parentToReturnTo = transform.parent;

        // 최상위 캔버스를 부모로 설정 (다른 UI 요소들보다 위에 그려지도록)
        transform.SetParent(rootCanvas);
        transform.SetAsLastSibling();

        // 3. (핵심) 레이캐스트를 막지 않도록 설정
        // 이게 true면, 이 이미지가 마우스 클릭을 다 먹어버려서
        // 뒤에 있는 슬롯(DropZone)의 OnDrop이 호출되지 않음
        canvasGroup.blocksRaycasts = false;

        GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
    }

    // 2. 드래그 중일 때 (매 프레임 호출)
    public void OnDrag(PointerEventData eventData)
    {
       RectTransform canvasRect = rootCanvas as RectTransform;
    if (canvasRect == null) return;

    Vector2 localPoint;

    // 스크린 좌표(eventData.position)를 캔버스의 로컬 좌표로 변환합니다.
    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera, // 렌더 모드에 맞는 카메라 (Overlay에선 null)
            out localPoint))
    {
        // 월드 포지션 대신 로컬 포지션을 설정합니다.
        transform.localPosition = localPoint;
    }
    }

    // 3. 드래그가 끝났을 때
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag");
        
        // 드롭에 성공했든 실패했든, 지정된 부모(parentToReturnTo)로 돌아감
        transform.SetParent(parentToReturnTo);
        
        // 슬롯의 중앙에 위치하도록 로컬 위치를 0으로
        GetComponent<RectTransform>().localPosition = Vector2.zero;

        // 레이캐스트 차단을 다시 켬
        canvasGroup.blocksRaycasts = true;
        GetComponent<RectTransform>().localScale = originalScale;
    }
}