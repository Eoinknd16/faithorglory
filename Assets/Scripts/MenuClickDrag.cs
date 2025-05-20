using UnityEngine;
using UnityEngine.EventSystems;

public class MenuClickDrag : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Optional: bring to front
        rectTransform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Move based on pointer delta, adjusted by canvas scale
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
}
