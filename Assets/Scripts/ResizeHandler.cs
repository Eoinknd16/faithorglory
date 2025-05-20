using UnityEngine;
using UnityEngine.EventSystems;

public class UniformScaleResizer : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public RectTransform targetPanel;         // The window to scale
    public float minScale = 0.5f;
    public float maxScale = 2.0f;

    private Vector2 startMousePos;
    private float startScale;
    private Canvas canvas;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        startMousePos = eventData.position;
        startScale = targetPanel.localScale.x;
    }

    public void OnDrag(PointerEventData eventData)
    {
        float dragDistance = (eventData.position - startMousePos).magnitude;
        float direction = Mathf.Sign(Vector2.Dot(eventData.position - startMousePos, new Vector2(1, 1)));

        float scaleDelta = direction * dragDistance * 0.002f; // Adjust sensitivity
        float newScale = Mathf.Clamp(startScale + scaleDelta, minScale, maxScale);

        targetPanel.localScale = Vector3.one * newScale;
    }
}
