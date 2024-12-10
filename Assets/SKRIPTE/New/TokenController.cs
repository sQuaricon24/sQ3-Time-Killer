using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class TokenController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private Vector3 initialPosition;
    private Vector3 dragStartPosition;
    [SerializeField] private int tokenId;
    [SerializeField] private bool canDragVertical = false;
    [SerializeField] private bool canDragHorizontal = false;

    [SerializeField]
    private float dragThreshold = 100f; // Minimum distance in pixels for a valid drag

    private RectTransform rectTransform;

    // Restriction flags
    //private bool allowHorizontal;
    //private bool allowVertical;

    // Lock the direction of the drag
    private bool isDirectionLocked = false;
    private Vector2 lockedDirection = Vector2.zero;
    private Vector3 lastKnownDeltaPosition = Vector3.zero;
    private bool isSpecialDebugEnabled = false;

    [SerializeField] private int gridPositionIndex;

    public int GridPositionIndex => gridPositionIndex;
    public void SetGridPositionIndex(int index)
    {
        gridPositionIndex = index;
    }

    // TO DO: refactor this naming and variable
    public void SetCanDragVertical(bool cdv)
    {
        canDragVertical = cdv;
    }

    public void SetCanDragHorizontal(bool cdh)
    {
        canDragHorizontal = cdh;
    }

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.localPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!GridManager.Instance.IsDraggingEnabled)
            return;

        dragStartPosition = rectTransform.localPosition;
        isDirectionLocked = false; // Reset direction lock at the start of each drag
        //Debug.LogError("ON BEGING DRAG");
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Debug.LogError("ON DRAG");
        if (!GridManager.Instance.IsDraggingEnabled)
            return;

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

            // Check if drag exceeds the threshold
            if (dragDelta.magnitude >= dragThreshold)
            {
                if (Mathf.Abs(dragDelta.x) > Mathf.Abs(dragDelta.y) && canDragHorizontal)
                {
                    lockedDirection = Vector2.right;
                    isDirectionLocked = true;
                }
                else if (Mathf.Abs(dragDelta.y) > Mathf.Abs(dragDelta.x) && canDragVertical)
                {
                    lockedDirection = Vector2.up;
                    isDirectionLocked = true;
                }
                else
                {
                    Debug.LogError("EEE NOO DIRECTION");
                }

                if (isDirectionLocked)
                {
                    GridManager.Instance.HandleBeginDrag(draggedToken: this, isVerticalDrag: lockedDirection == Vector2.up);
                }
            }
        }

        // Apply constraints based on the locked direction
        if (isDirectionLocked)
        {
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

            Vector3 deltaPosition = newPosition - rectTransform.localPosition;
            deltaPosition = deltaPosition.normalized * 5;
            //Debug.LogError("deltaPosition " + deltaPosition);
            UpdateLocalPosition(deltaPosition);
            lastKnownDeltaPosition = deltaPosition;

            GridManager.Instance.HandleTokenDragFrame(draggedToken: this, dragDeltaForCurrentFrame: deltaPosition);
        }
    }

    void Update()
    {
        if(Input.GetKeyUp(KeyCode.Space))
        {
            isSpecialDebugEnabled = !isSpecialDebugEnabled;
        }

        Vector2 overflowDirection = GridManager.Instance.GetOverflowDirection(rectTransform.localPosition);
        if (overflowDirection != Vector2.zero)
        {
            if ((isSpecialDebugEnabled))
            {
                Debug.LogError("OverflowDirection " + overflowDirection);
            }
            //Debug.LogError("gridPositionIndex " + gridPositionIndex);
            HandleTokenOverflow(overflowDirection);
        }
    }

    public void UpdateLocalPosition(Vector3 deltaLocalPosition)
    {
        //Debug.LogError("deltaLocalPosition " + deltaLocalPosition);
        rectTransform.localPosition += deltaLocalPosition;
    }

    public void PlayDwindleAnimation(Vector2 dir, Vector3 targetPosition, float magnitude = 10f, float duration = 0.2f)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector3 originalPosition = rectTransform.localPosition;

        // Define the dwindle sequence
        Sequence dwindleSequence = DOTween.Sequence();

        dwindleSequence.Append(rectTransform.DOAnchorPos(originalPosition + (Vector3)(dir * magnitude), duration).SetEase(Ease.OutQuad))
                       .Append(rectTransform.DOAnchorPos(originalPosition - (Vector3)(dir * magnitude * 0.5f), duration * 0.75f).SetEase(Ease.OutQuad))
                       .Append(rectTransform.DOAnchorPos(originalPosition + (Vector3)(dir * magnitude * 0.3f), duration * 0.5f).SetEase(Ease.OutQuad))
                       .Append(rectTransform.DOAnchorPos(originalPosition - (Vector3)(dir * magnitude * 0.2f), duration * 0.4f).SetEase(Ease.OutQuad))
                       .Append(rectTransform.DOAnchorPos(originalPosition, duration * 0.3f).SetEase(Ease.OutQuad))
                       .OnComplete(() => rectTransform.localPosition = targetPosition);
    }

    private void HandleTokenOverflow(Vector2 overflowDirection)
    {
      //  Debug.LogError("overflowDirection " + overflowDirection);
        //Debug.LogError("rectTransform.localPosition before " + rectTransform.localPosition); 
        if (overflowDirection == Vector2.up)
            rectTransform.localPosition -= new Vector3(0, 900, 0);
        else if (overflowDirection == Vector2.down)
            rectTransform.localPosition += new Vector3(0, 900, 0);
        else if (overflowDirection == Vector2.right)
            rectTransform.localPosition -= new Vector3(900, 0, 0);
        else if (overflowDirection == Vector2.left)
            rectTransform.localPosition += new Vector3(900, 0, 0);

       // Debug.LogError("rectTransform.localPosition after " + rectTransform.localPosition);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!GridManager.Instance.IsDraggingEnabled)
            return;

        //Debug.LogError("ON END DRAG");

        Vector3 dragEndPosition = rectTransform.localPosition;
        Vector3 dragDelta = dragEndPosition - dragStartPosition;

        StartCoroutine(SnapToGrid());
        GridManager.Instance.HandleDragEnd();
    }

    private Vector2 GetDominantDirection(Vector3 delta)
    {
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            return canDragHorizontal ? new Vector2(Mathf.Sign(delta.x), 0) : Vector2.zero;
        }
        else
        {
            return canDragVertical ? new Vector2(0, Mathf.Sign(delta.y)) : Vector2.zero;
        }
    }

    public IEnumerator SnapToGrid(float delay = 0f)
    {
        yield return new WaitForSeconds(delay);
        Vector3 nearestPosition = GridManager.Instance.GetNearestGridPosition(rectTransform.position);
        SmoothSnap(nearestPosition);
    }

    private Vector3 snapDirection;
    private void SmoothSnap(Vector3 targetPosition, float snapDuration = 0.3f)
    {
        RectTransform parentRect = rectTransform.parent.GetComponent<RectTransform>();
        Vector3 localTargetPosition = parentRect.InverseTransformPoint(targetPosition);
        snapDirection = localTargetPosition - rectTransform.localPosition;
        // Use DoTween for smooth snapping with easing
        rectTransform.DOLocalMove(localTargetPosition, snapDuration)
                     .SetEase(Ease.InCubic) // Non-linear easing: slow start, fast finish
                     .OnComplete(() =>
                     {
                         rectTransform.localPosition = localTargetPosition;
                         initialPosition = localTargetPosition;
                         HandleOnSnapComplete(snapDirection, localTargetPosition);
                     });
    }

    private void HandleOnSnapComplete(Vector3 snapDirection, Vector3 targetPosition)
    {
        PlayDwindleAnimation(snapDirection.normalized, targetPosition);
    }
}
