using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Grid Settings")]
    public List<GameObject> tokens;       // References to all token GameObjects
    public List<Vector3> gridPositions;  // Predefined positions for the grid
    public float moveDuration = 0.5f;    // Duration of the movement animation

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InitializeGrid();
    }

    // Initializes grid positions based on the UI RectTransform
    private void InitializeGrid()
    {
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
    private List<TokenController> nonDraggedTokensToBeAffectedByDrag;
    // delay is applied to every token that follows movement of dragged token, but delay is multiplied for each token-to-token distance
    // again, this additive delay is used to achieve "vagons in the train" effect
    private float delay = 0.2f;

    public void HandleBeginDrag(TokenController draggedToken, bool isVerticalDrag)
    {
        // when new drag action is stared, we fetch tokens that will be affected by this
        FetchNonDraggedTokensToBeAffectedByDrag(draggedToken: draggedToken, isVerticalDrag: isVerticalDrag);
    }

    public void HandleTokenDragFrame(TokenController draggedToken, Vector2 dragDeltaForCurrentFrame)
    {
        // each frame a token is dragged, apply movement to other tokens that are affected by its movement
        foreach (TokenController tc in nonDraggedTokensToBeAffectedByDrag)
        {
            //tc.UpdateLocalPosition(dragDeltaForCurrentFrame);
        }
    }

    private void FetchNonDraggedTokensToBeAffectedByDrag(TokenController draggedToken, bool isVerticalDrag)
    {
        nonDraggedTokensToBeAffectedByDrag = new List<TokenController>();

        // Get the position of the dragged token
        Vector3 draggedPosition = draggedToken.transform.position;

        // Define the direction of raycasting
        Vector2 direction = isVerticalDrag ? Vector2.up : Vector2.right;

        // Perform raycasting in both positive and negative directions
        float maxDistance = 100f; // Arbitrary high value to ensure all relevant tokens are detected

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
        if (token != null && token != draggedToken)
        {
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

    // Snaps all tokens to their nearest grid positions
    public void SnapTokensToGrid()
    {
        foreach (var token in tokens)
        {
            Vector3 targetPosition = GetNearestGridPosition(token.transform.position);
            StartCoroutine(MoveTokenToPosition(token, targetPosition));
        }
    }


    // Moves a token to a specific grid position with animation
    private IEnumerator MoveTokenToPosition(GameObject token, Vector3 targetPosition)
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

    // Handles drag-based token movement
    public void HandleTokenDrag(TokenController token, Vector2 dragDirection)
    {
        Debug.Log($"Drag direction received: {dragDirection}");
        // Additional logic for dragging rows or columns can be added here
    }
}
