using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Squaricon.SQ3
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance;

        [Header("Grid Settings")]
        [SerializeField] private float dragSpeed = 5f;
        public List<TokenController> tokens;       // References to all token GameObjects
        public List<Vector3> gridPositions;  // Predefined positions for the grid
        public float moveDuration = 0.5f;    // Duration of the movement animation
        private bool isDraggingEnabled = true;

        public bool IsDraggingEnabled => isDraggingEnabled;

        public float DragSpeed => dragSpeed;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            InitializeGrid();
            SetGridPositionIndexesAccordingToPositionForAllTokens();
        }

        // Initializes grid positions based on the UI RectTransform
        private void InitializeGrid()
        {
            tokens = new List<TokenController>(GetComponentsInChildren<TokenController>());
            gridPositions = new List<Vector3>();

            RectTransform rectTransform = GetComponent<RectTransform>();
            Vector2 gridSize = rectTransform.rect.size;
            float cellSize = gridSize.x / 3; // Assuming a 3x3 grid

            // Generate positions for a 3x3 grid
            for (int y = 0; y < 3; y++) // Rows
            {
                for (int x = 0; x < 3; x++) // Columns
                {
                    Vector3 position = new Vector3(
                        -gridSize.x / 2 + (x + 0.5f) * cellSize,
                        -gridSize.y / 2 + (y + 0.5f) * cellSize,
                        0
                    );
                    gridPositions.Add(position);
                }
            }
        }

        // tokens in the same row or column, depending if draging vertical or horizontal
        // these tokens will follow drag movement of dragged token, but with delay to achieve "vagons in the train" effect
        [SerializeField] private List<TokenController> nonDraggedTokensToBeAffectedByDrag;
        // delay is applied to every token that follows movement of dragged token, but delay is multiplied for each token-to-token distance
        // again, this additive delay is used to achieve "vagons in the train" effect
        private float delay = 0.2f;

        // if row 1 is moved left, row 3 needs do be moved right, and vice versa, inverted mirror movement
        // in same way, if column 1 is moved up, column 3 needs to be moved down
        [SerializeField] private List<TokenController> oppisiteTokensAffectedByDrag;

        public void HandleBeginDrag(TokenController draggedToken, bool isVerticalDrag)
        {
            // when new drag action is stared, we fetch tokens that will be affected by this
            FetchNonDraggedTokensToBeAffectedByDrag(draggedToken: draggedToken, isVerticalDrag: isVerticalDrag);
            FetchOppisiteTokensAffectedByDrag(draggedToken: draggedToken, isVerticalDrag: isVerticalDrag);
        }

        private void FetchOppisiteTokensAffectedByDrag(TokenController draggedToken, bool isVerticalDrag)
        {
            oppisiteTokensAffectedByDrag = new List<TokenController>();
            int tokenIndex;
            int oppositeIndex;
            TokenController fetchedTc;
            foreach (TokenController tc in nonDraggedTokensToBeAffectedByDrag)
            {
                tokenIndex = GetTokenIndexFromPosition(tc.GetComponent<RectTransform>().localPosition);
                oppositeIndex = GetOppositeTokenGridPositionIndexForMovementDirection(isVerticalDrag, tokenIndex);
                fetchedTc = GetTokenByGridPositionIndex(oppositeIndex);
                if (fetchedTc == null)
                {
                    Debug.LogError("NULL FOR INDEX " + oppositeIndex);
                }
                oppisiteTokensAffectedByDrag.Add(fetchedTc);
            }

            tokenIndex = GetTokenIndexFromPosition(draggedToken.GetComponent<RectTransform>().localPosition);
            oppositeIndex = GetOppositeTokenGridPositionIndexForMovementDirection(isVerticalDrag, tokenIndex);
            fetchedTc = GetTokenByGridPositionIndex(oppositeIndex);
            if (fetchedTc == null)
            {
                Debug.LogError("NULL FOR INDEX " + oppositeIndex);
            }
            oppisiteTokensAffectedByDrag.Add(fetchedTc);
        }

        private TokenController GetTokenByGridPositionIndex(int index)
        {
            return tokens.FirstOrDefault(x => x.GridPositionIndex == index);
        }

        private void SetGridPositionIndexesAccordingToPositionForAllTokens()
        {
            foreach (TokenController tc in tokens)
            {
                tc.SetGridPositionIndex(GetTokenIndexFromPosition(tc.GetComponent<RectTransform>().localPosition));
            }

            isDraggingEnabled = true;
        }

        public void HandleTokenDragFrame(TokenController draggedToken, Vector2 dragDeltaForCurrentFrame, DragAxis activeDragAxis)
        {
            if (nonDraggedTokensToBeAffectedByDrag == null)
                FetchNonDraggedTokensToBeAffectedByDrag(draggedToken);
            if (oppisiteTokensAffectedByDrag == null)
                FetchOppisiteTokensAffectedByDrag();



            StartCoroutine(AnimateTokensWithDelay(nonDraggedTokensToBeAffectedByDrag, dragDeltaForCurrentFrame, 0.05f));
            StartCoroutine(AnimateTokensWithDelay(oppisiteTokensAffectedByDrag, -dragDeltaForCurrentFrame, 0.05f));
        }

        private IEnumerator AnimateTokensWithDelay(List<TokenController> tokens, Vector2 dragDelta, float delayPerToken)
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                //yield return new WaitForSeconds(delayPerToken);
                yield return null;
                tokens[i].AddToLocalPosition(dragDelta);
            }
        }


        private void FetchNonDraggedTokensToBeAffectedByDrag(TokenController draggedToken, DragAxis dragAxis)
        {
            nonDraggedTokensToBeAffectedByDrag = new List<TokenController>();

            // Get the position of the dragged token
            Vector3 draggedPosition = draggedToken.transform.position;

            // Define the direction of raycasting
            Vector2 direction = dragAxis == DragAxis.Vertical ? Vector2.up : Vector2.right;

            // Perform raycasting in both positive and negative directions
            float maxDistance = 1000f; // Arbitrary high value to ensure all relevant tokens are detected

            // TO DO: differentiate between first token in direction and opposite direction because we need to know direction in which
            // we propagate delay


            // Cast in the positive direction
            RaycastHit2D[] hitsPositive = Physics2D.RaycastAll(draggedPosition, direction, maxDistance);

            // Cast in the negative direction
            RaycastHit2D[] hitsNegative = Physics2D.RaycastAll(draggedPosition, -direction, maxDistance);

            // Collect all tokens hit by the rays
            foreach (var hit in hitsPositive)
            {
                AddTokenIfValid(hit, draggedToken);
            }

            foreach (var hit in hitsNegative)
            {
                AddTokenIfValid(hit, draggedToken);
            }
        }

        private void AddTokenIfValid(RaycastHit2D hit, TokenController draggedToken)
        {
            // Check if the hit object is a token and not the dragged token
            TokenController token = hit.collider.GetComponent<TokenController>();
            if (token != null && token != draggedToken && token.name != draggedToken.name)
            {
                if (token == null)
                {
                    Debug.LogError("TOKEN IS NULL");
                }
                nonDraggedTokensToBeAffectedByDrag.Add(token);
            }
        }


        // Finds the nearest grid position for snapping
        public Vector3 GetNearestGridPosition(Vector3 currentPosition)
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            Vector3 localPosition = rectTransform.InverseTransformPoint(currentPosition);

            float minDistance = float.MaxValue;
            Vector3 nearestPosition = Vector3.zero;

            foreach (var gridPosition in gridPositions)
            {
                float distance = Vector3.Distance(localPosition, gridPosition);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestPosition = gridPosition;
                }
            }

            return rectTransform.TransformPoint(nearestPosition);
        }


        // Moves a token to a specific grid position with animation
        private IEnumerator MoveTokenToPosition(TokenController token, Vector3 targetPosition)
        {
            RectTransform rectTransform = token.GetComponent<RectTransform>();
            Vector3 startPosition = rectTransform.localPosition;
            RectTransform parentRect = GetComponent<RectTransform>();
            Vector3 localTargetPosition = parentRect.InverseTransformPoint(targetPosition);

            float elapsedTime = 0f;

            while (elapsedTime < moveDuration)
            {
                elapsedTime += Time.deltaTime;
                rectTransform.localPosition = Vector3.Lerp(startPosition, localTargetPosition, elapsedTime / moveDuration);
                yield return null;
            }

            rectTransform.localPosition = localTargetPosition;
        }

        public void HandleDragEnd()
        {
            isDraggingEnabled = false;

            float cumulativeDelay = 0;
            //Debug.LogError("Handle drag end");
            foreach (TokenController tc in nonDraggedTokensToBeAffectedByDrag)
            {
                cumulativeDelay += 0.05f;
                StartCoroutine(tc.SnapToGrid(cumulativeDelay));
            }

            foreach (TokenController tc in oppisiteTokensAffectedByDrag)
            {
                cumulativeDelay += 0.05f;
                StartCoroutine(tc.SnapToGrid(cumulativeDelay));
            }

            // Invoke("PlayDwinldeAnimations", 0.4f);
            // TO DO: refactor
            Invoke("UpdateDragConstraintsForEachToken", 1.4f);
            Invoke("SetGridPositionIndexesAccordingToPositionForAllTokens", 1.5f);
            //UpdateDragConstraintsForEachToken();
            // trigger OnMoveFinished event so other script, GridLogic maybe, can apply logic
        }


        private void UpdateDragConstraintsForEachToken()
        {
            foreach (TokenController tc in tokens)
            {
                int tokenIndex = GetTokenIndexFromPosition(tc.GetComponent<RectTransform>().localPosition);
                tc.SetCanDragHorizontal(CanDragHorizontalForGridPositionIndex(tokenIndex));
                tc.SetCanDragVertical(CanDragVerticalForGridPositionIndex(tokenIndex));
            }
        }




        private int GetTokenIndexFromPosition(Vector2 positionInGrid)
        {
            //0,1,2
            //3,4,5
            //6,7,8

            //TO DO: make dynamic

            // Define grid centers for rows and columns
            float[] columns = { -300f, 0f, 300f };
            float[] rows = { 300f, 0f, -300f };

            // Find the closest column and row indices
            int colIndex = GetClosestIndex(positionInGrid.x, columns);
            int rowIndex = GetClosestIndex(positionInGrid.y, rows);

            // Validate indices and calculate the token index
            if (colIndex != -1 && rowIndex != -1)
            {
                return rowIndex * columns.Length + colIndex;
            }

            Debug.LogError($"Invalid position in grid: {positionInGrid}");
            return -1; // Return -1 if the position is outside the grid
        }

        private int GetClosestIndex(float value, float[] referencePoints)
        {
            int closestIndex = -1;
            float minDistance = float.MaxValue;

            for (int i = 0; i < referencePoints.Length; i++)
            {
                float distance = Mathf.Abs(value - referencePoints[i]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                }
            }
            return closestIndex;
        }

        // TO DO: try to make dynamic
        private bool CanDragVerticalForGridPositionIndex(int positionIndex)
        {
            switch (positionIndex)
            {
                case (0): return true;
                case (1): return false;
                case (2): return true;
                case (3): return true;
                case (4): return false;
                case (5): return true;
                case (6): return true;
                case (7): return false;
                case (8): return true;
                default: Debug.LogError("NOT VALID POS INDEX 1"); return false;
            }
        }

        // TO DO: try to make dynamic
        private bool CanDragHorizontalForGridPositionIndex(int positionIndex)
        {
            switch (positionIndex)
            {
                case (0): return true;
                case (1): return true;
                case (2): return true;
                case (3): return false;
                case (4): return false;
                case (5): return false;
                case (6): return true;
                case (7): return true;
                case (8): return true;
                default: Debug.LogError("NOT VALID POS INDEX 2"); return false;
            }
        }

        private int GetOppositeTokenGridPositionIndexForMovementDirection(bool isVertical, int positionIndex)
        {
            switch (positionIndex)
            {
                case (0): return isVertical ? 2 : 6;
                case (1): return isVertical ? -1 : 7;
                case (2): return isVertical ? 0 : 8;
                case (3): return isVertical ? 5 : -1;
                case (4): return isVertical ? -1 : -1;
                case (5): return isVertical ? 3 : -1;
                case (6): return isVertical ? 8 : 0;
                case (7): return isVertical ? -1 : 1;
                case (8): return isVertical ? 6 : 2;
                default: Debug.LogError("NOT VALID POS INDEX 3"); return -1;
            }
        }


        public Vector2 GetOverflowDirection(Vector2 position)
        {
            // Get the grid's RectTransform
            RectTransform rectTransform = GetComponent<RectTransform>();
            // Token position is already in local space for Screen Space - Overlay
            Vector2 localPosition = position;

            //Debug.LogError("localPosition " + localPosition);
            // Define the grid boundaries
            float halfWidth = rectTransform.rect.width / 2;
            float halfHeight = rectTransform.rect.height / 2;

            // Check for overflow and return the appropriate direction
            if (localPosition.x <= -halfWidth) return Vector2.left;
            if (localPosition.x >= halfWidth) return Vector2.right;
            if (localPosition.y <= -halfHeight) return Vector2.down;
            if (localPosition.y >= halfHeight) return Vector2.up;

            // No overflow detected
            return Vector2.zero;
        }
    }
}
