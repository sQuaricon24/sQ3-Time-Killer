using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using FirstCollection;

public class Tile : MonoBehaviour, IBeginDragHandler, IDragHandler, IPointerClickHandler
{
    GameManager gm;

    [SerializeField] private bool canBeDraged = true;
    [SerializeField] private Image image;
    [SerializeField] private int startingTileSpriteIndex;
    public AllowedDirection allowedDirection;

    private int tileSpriteIndex;
    private Vector2Int myPosition;
    private Vector2Int oppositePosition;
    private Vector2 dragDelta;
    private Vector2Int moveDir;

    public int TileSpriteIndex
    {
        get => tileSpriteIndex;
        set
        {
            tileSpriteIndex = value;
            image.sprite = SoSetting.Instance.tileSprites[tileSpriteIndex];
        }
    }

    public Vector2Int MyPosition
    {
        get => myPosition;
        set
        {
            myPosition = value;
            gameObject.name = $"Tile {myPosition.x} {myPosition.y}";
            oppositePosition = new Vector2Int((myPosition.x + 2) % 2, (myPosition.y + 2) % 2); //pogresno, ispraviti
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!canBeDraged) return;

        dragDelta = eventData.delta;
        if (allowedDirection == AllowedDirection.MiddlePoint)
        {
            return;
        }

        if (Mathf.Abs(dragDelta.x) > Mathf.Abs(dragDelta.y))
        {
            if (allowedDirection == AllowedDirection.Vertical) return;

            moveDir = new Vector2Int(dragDelta.x > 0 ? 1 : -1, 0);
        }
        else if (Mathf.Abs(dragDelta.x) < Mathf.Abs(dragDelta.y))
        {
            if (allowedDirection == AllowedDirection.Horizontal) return;

            moveDir = new Vector2Int(0, dragDelta.y > 0 ? 1 : -1);
        }
        else return;

        gm.MoveTokenByPosition(moveDir, MyPosition);

        Vector2Int oppositePosition = gm.OppositePos(moveDir, MyPosition);

        gm.MoveTokenByPosition(-moveDir, oppositePosition);

        gm.HandleMoveFinished();
    }

    public void OnDrag(PointerEventData eventData)
    {

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (allowedDirection != AllowedDirection.MiddlePoint) return;
        gm.MiddlePointActivation(new Vector2Int(1, 1));
        gm.HandleMoveFinished();
    }

    private void Awake()
    {
        gm = GameManager.Instance;
        if(SoSetting.Instance.IsAdventureMode)
        {
            Color c = image.color;
            c.a = 0;
            image.color = c;
        }
    }

    void OnEnable()
    {
        SquariconGlobalEvents.OnSkinUpdated += HandleSkinUpdated;
    }

    void OnDisable()
    {
        SquariconGlobalEvents.OnSkinUpdated -= HandleSkinUpdated;
    }

    private void Start()
    {
        TileSpriteIndex = startingTileSpriteIndex;
    }

    private void HandleSkinUpdated()
    {
        TileSpriteIndex = TileSpriteIndex;
    }
}
