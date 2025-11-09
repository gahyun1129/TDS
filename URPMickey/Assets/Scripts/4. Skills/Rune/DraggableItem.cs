using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class DraggableItem : MonoBehaviour
{
    [Header("드래그 정보")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rune rune;
    
    private Vector3 offset;
    private string originalSortingLayerName;
    private int originalSortingOrder;
    private Vector3 originalPosition;

    private GraphicRaycaster graphicRaycaster;
    private EventSystem eventSystem;

    void Start()
    {
        GameObject canvas = GameObject.FindWithTag("UI_Canvas_Target");
        if (canvas != null)
        {
            graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
        }
        eventSystem = EventSystem.current;
    }

    void OnMouseDown()
    {
        originalPosition = transform.position;
        originalSortingLayerName = spriteRenderer.sortingLayerName;
        originalSortingOrder = spriteRenderer.sortingOrder;

        spriteRenderer.sortingLayerName = "Dragging";
        spriteRenderer.sortingOrder = 10;

        rune.OnSelected();
        offset = transform.position - GetMouseWorldPos();
    }

    void OnMouseDrag()
    {
        transform.position = GetMouseWorldPos() + offset;
    }

    void OnMouseUp()
    {
        rune.OnDropped();
        spriteRenderer.sortingLayerName = originalSortingLayerName;
        spriteRenderer.sortingOrder = originalSortingOrder;

        PointerEventData eventData = new PointerEventData(eventSystem);
        eventData.position = Input.mousePosition;

        if (graphicRaycaster == null)
        {
            transform.position = originalPosition;
            return;
        }

        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(eventData, results);

        foreach (RaycastResult result in results)
        {
            InvenSlotUI dropZone = result.gameObject.GetComponent<InvenSlotUI>();
            if (dropZone != null)
            {
                dropZone.EquippedRune(gameObject);
                BattleManager.Instance.RemoveCog(gameObject);
                return;
            }
        }

        transform.position = originalPosition;
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
}
