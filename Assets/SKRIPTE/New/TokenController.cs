using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class TokenController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private Vector3 initialPosition;
    private Vector3 dragStartPosition;

    [SerializeField]
    private float dragThreshold = 50f; // Minimum distance in pixels for a valid drag

    private RectTransform rectTransform;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.localPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragStartPosition = rectTransform.localPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)transform.parent,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPosition
        );

        rectTransform.localPosition = localPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Vector3 dragEndPosition = rectTransform.localPosition;
        Vector3 dragDelta = dragEndPosition - dragStartPosition;

        if (dragDelta.magnitude > dragThreshold)
        {
            Vector2 dragDirection = GetDominantDirection(dragDelta);
            GridManager.Instance.HandleTokenDrag(this, dragDirection);
        }

        SnapToGrid();
    }

    private Vector2 GetDominantDirection(Vector3 delta)
    {
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            return new Vector2(Mathf.Sign(delta.x), 0); // Horizontal
        }
        else
        {
            return new Vector2(0, Mathf.Sign(delta.y)); // Vertical
        }
    }

    private void SnapToGrid()
    {
        Vector3 nearestPosition = GridManager.Instance.GetNearestGridPosition(rectTransform.position);
        StartCoroutine(SmoothSnap(nearestPosition));
    }

    private IEnumerator SmoothSnap(Vector3 targetPosition)
    {
        Vector3 startPosition = rectTransform.localPosition;
        RectTransform parentRect = rectTransform.parent.GetComponent<RectTransform>();
        Vector3 localTargetPosition = parentRect.InverseTransformPoint(targetPosition);

        float elapsedTime = 0f;
        float snapDuration = 0.3f; // Smooth snap duration

        while (elapsedTime < snapDuration)
        {
            elapsedTime += Time.deltaTime;
            rectTransform.localPosition = Vector3.Lerp(startPosition, localTargetPosition, elapsedTime / snapDuration);
            yield return null;
        }

        rectTransform.localPosition = localTargetPosition;
        initialPosition = localTargetPosition;
    }
}
