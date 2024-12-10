using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Squaricon.SQ3
{
    public partial class TokenController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] private int tokenId;

        [Tooltip("Will get overriden in runtime, just for inspection")]
        [SerializeField] private bool canDragVertical = false;
        [SerializeField] private bool canDragHorizontal = false;
        [SerializeField] private int gridPositionIndex;
        [Space(10)]
        [SerializeField] private float dragThreshold = 50f; // Minimum distance in pixels for a valid drag start

        private RectTransform myRectTransform;
        private Vector3 initialPosition;
        private Vector3 dragStartPosition;
        // We allow only straight vertical and horizontal drag
        private bool isDragAxisDetermined = false;
        private DragAxis activeDragAxis = DragAxis.None;
        private float dragSpeed;
        private Vector2 snapDirection;


        public int GridPositionIndex => gridPositionIndex;


        public void SetGridPositionIndex(int index)
        {
            gridPositionIndex = index;
        }

        public void SetCanDragVertical(bool cdv)
        {
            canDragVertical = cdv;
        }

        public void SetCanDragHorizontal(bool cdh)
        {
            canDragHorizontal = cdh;
        }
        public void AddToLocalPosition(Vector3 deltaLocalPosition)
        {
            myRectTransform.localPosition += deltaLocalPosition;
        }

        public IEnumerator SnapToGrid(float delay = 0f)
        {
            yield return new WaitForSeconds(delay);
            Vector3 nearestPosition = GridManager.Instance.GetNearestGridPosition(myRectTransform.position);
            SmoothSnap(nearestPosition);
        }


        #region DRAGGING LOGIC
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!GridManager.Instance.IsDraggingEnabled)
                return;

            dragStartPosition = myRectTransform.localPosition;
            isDragAxisDetermined = false; // we need to determine the axis on start of each drag
        }

        public void OnDrag(PointerEventData eventData)
        {
            //Debug.LogError("ON DRAG");
            if (!GridManager.Instance.IsDraggingEnabled)
                return;

            // map screen point to coordinate inside a grid to determine where user tapped
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)transform.parent,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPosition
            );

            Vector3 newPosition = myRectTransform.localPosition;

            // Determine drag direction
            if (!isDragAxisDetermined)
            {
                Vector3 dragDelta = localPosition - (Vector2)dragStartPosition;

                // Check if drag exceeds the threshold to be actually registered as a drag and not as just finger placement
                if (dragDelta.magnitude >= dragThreshold)
                {
                    if (Mathf.Abs(dragDelta.x) > Mathf.Abs(dragDelta.y) && canDragHorizontal)
                    {
                        activeDragAxis = DragAxis.Horizontal;
                        isDragAxisDetermined = true;
                    }
                    else if (Mathf.Abs(dragDelta.y) > Mathf.Abs(dragDelta.x) && canDragVertical)
                    {
                        activeDragAxis = DragAxis.Vertical;
                        isDragAxisDetermined = true;
                    }
                    else
                    {
                        Debug.LogError("WAS NOT ABLE TO DETERMINE DRAG AXIS");
                    }

                    if (isDragAxisDetermined)
                    {
                        SquariconGlobalEvents.OnTokenDragStarted(this, activeDragAxis);
                        //GridManager.Instance.HandleBeginDrag(draggedToken: this, isVerticalDrag: activeDragAxis == DragAxis.Vertical);
                    }
                }
            }

            // Apply constraints based on the locked direction
            if (isDragAxisDetermined)
            {
                if (activeDragAxis == DragAxis.Horizontal)
                {
                    newPosition.x = localPosition.x; // Allow horizontal movement
                    newPosition.y = dragStartPosition.y; // Lock vertical position
                }
                else if (activeDragAxis == DragAxis.Vertical)
                {
                    newPosition.y = localPosition.y; // Allow vertical movement
                    newPosition.x = dragStartPosition.x; // Lock horizontal position
                }

                Vector3 deltaPosition = newPosition - myRectTransform.localPosition;
                deltaPosition = deltaPosition.normalized * dragSpeed;

                // Grid manager determines which tokens should be dragged, this invoking token will be draged but movement will be
                // replicated to other tokens according to game logic
                SquariconGlobalEvents.OnTokenDragFrameCompleted(this, (Vector2)deltaPosition, activeDragAxis);
                //GridManager.Instance.HandleTokenDragFrame(draggedToken: this, dragDeltaForCurrentFrame: deltaPosition);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!GridManager.Instance.IsDraggingEnabled)
                return;

            GridManager.Instance.HandleDragEnd();
        }
        #endregion


        private void Awake()
        {
            myRectTransform = GetComponent<RectTransform>();
            initialPosition = myRectTransform.localPosition;
        }

        private void Start()
        {
            dragSpeed = GridManager.Instance.DragSpeed;
        }

        private void Update()
        {
            HandleTokenOverflow();
        }



        private void HandleTokenOverflow()
        {
            Vector2 overflowDirection = GridManager.Instance.GetOverflowDirection(myRectTransform.localPosition);
            if (overflowDirection != Vector2.zero)
            {
                KickTokenToTheOtherSide(overflowDirection);
            }
        }

        // TO DO: solve magic numbers
        private void KickTokenToTheOtherSide(Vector2 overflowDirection)
        {
            if (overflowDirection == Vector2.up)
                myRectTransform.localPosition -= new Vector3(0, 900, 0);
            else if (overflowDirection == Vector2.down)
                myRectTransform.localPosition += new Vector3(0, 900, 0);
            else if (overflowDirection == Vector2.right)
                myRectTransform.localPosition -= new Vector3(900, 0, 0);
            else if (overflowDirection == Vector2.left)
                myRectTransform.localPosition += new Vector3(900, 0, 0);
            else
                Debug.LogError("INVALID STATE IN KickTokenToTheOtherSide");
        }

        // TO DO: solve magic numbers
        private void PlayDwindleAnimation(Vector2 initialAnimDir, Vector3 positionAfterAnimation, float magnitude = 10f, float duration = 0.2f)
        {
            Vector3 initialPos = myRectTransform.localPosition;

            // Define the dwindle sequence
            Sequence dwindleSequence = DOTween.Sequence();

            dwindleSequence.Append(myRectTransform.DOAnchorPos(initialPos + (Vector3)(initialAnimDir * magnitude), duration).SetEase(Ease.OutQuad))
                           .Append(myRectTransform.DOAnchorPos(initialPos - (Vector3)(initialAnimDir * magnitude * 0.5f), duration * 0.75f).SetEase(Ease.OutQuad))
                           .Append(myRectTransform.DOAnchorPos(initialPos + (Vector3)(initialAnimDir * magnitude * 0.3f), duration * 0.5f).SetEase(Ease.OutQuad))
                           .Append(myRectTransform.DOAnchorPos(initialPos - (Vector3)(initialAnimDir * magnitude * 0.2f), duration * 0.4f).SetEase(Ease.OutQuad))
                           .Append(myRectTransform.DOAnchorPos(initialPos, duration * 0.3f).SetEase(Ease.OutQuad))
                           .OnComplete(() => myRectTransform.localPosition = positionAfterAnimation);
        }

        private void SmoothSnap(Vector2 snapTargetPosition, float snapDuration = 0.3f)
        {
            RectTransform parentRect = myRectTransform.parent.GetComponent<RectTransform>();
            Vector2 localSnapTargetPosition = parentRect.InverseTransformPoint(snapTargetPosition);
            snapDirection = (localSnapTargetPosition - (Vector2)myRectTransform.localPosition).normalized;
            // Use DoTween for smooth snapping with easing
            myRectTransform.DOLocalMove(localSnapTargetPosition, snapDuration)
                         .SetEase(Ease.InCubic) // Non-linear easing: slow start, fast finish
                         .OnComplete(() =>
                         {
                             myRectTransform.localPosition = localSnapTargetPosition;
                             initialPosition = localSnapTargetPosition;
                             PlayDwindleAnimation(snapDirection, positionAfterAnimation:localSnapTargetPosition);
                         });
        }
    }
}
