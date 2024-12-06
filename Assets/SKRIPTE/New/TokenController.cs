using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class TokenController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private Vector3 initialPosition;
    private Vector3 dragStartPosition;
    [SerializeField] private bool canDragVertical = false;
    [SerializeField] private bool canDragHorizontal = false;

    [SerializeField]
    private float dragThreshold = 50f; // Minimum distance in pixels for a valid drag

    private RectTransform rectTransform;

    // Restriction flags
    private bool allowHorizontal;
    private bool allowVertical;

    // Lock the direction of the drag
    private bool isDirectionLocked = false;
    private Vector2 lockedDirection = Vector2.zero;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.localPosition;
        allowHorizontal = canDragHorizontal;
        allowVertical = canDragVertical;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragStartPosition = rectTransform.localPosition;
        isDirectionLocked = false; // Reset direction lock at the start of each drag
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)transform.parent,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPosition
        );

        Vector3 newPosition = rectTransform.localPosition;

        // Determine drag direction and lock it if not already locked
        if (!isDirectionLocked)
        {
            Vector3 dragDelta = localPosition - (Vector2)dragStartPosition;

            if (Mathf.Abs(dragDelta.x) > Mathf.Abs(dragDelta.y) && allowHorizontal)
            {
                lockedDirection = Vector2.right;
                isDirectionLocked = true;
            }
            else if (Mathf.Abs(dragDelta.y) > Mathf.Abs(dragDelta.x) && allowVertical)
            {
                lockedDirection = Vector2.up;
                isDirectionLocked = true;
            }
        }

        // Apply constraints based on the locked direction
        if (lockedDirection == Vector2.right)
        {
            newPosition.x = localPosition.x; // Allow horizontal movement
            newPosition.y = dragStartPosition.y; // Lock vertical position
        }
        else if (lockedDirection == Vector2.up)
        {
            newPosition.y = localPosition.y; // Allow vertical movement
            newPosition.x = dragStartPosition.x; // Lock horizontal position
        }

        rectTransform.localPosition = newPosition;
    }

    public void UpdateLocalPosition(Vector3 deltaLocalPosition)
    {
        rectTransform.localPosition += deltaLocalPosition;
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
            return allowHorizontal ? new Vector2(Mathf.Sign(delta.x), 0) : Vector2.zero;
        }
        else
        {
            return allowVertical ? new Vector2(0, Mathf.Sign(delta.y)) : Vector2.zero;
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
