using UnityEngine;
using UnityEngine.EventSystems;

public class IChangeSize : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [Header("크기 조절")]
    [SerializeField] private float value;

    private Vector3 originalSize;
    private RectTransform rect;
    
    void Start()
    {
        rect = GetComponent<RectTransform>();
        originalSize = rect.localScale;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        rect.localScale = originalSize * value;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        rect.localScale = originalSize;
    }

}
