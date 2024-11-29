using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using FirstCollection;

public class Tile : MonoBehaviour, IBeginDragHandler, IDragHandler, IPointerClickHandler
{
    GameManager gm;

    [SerializeField] private bool canBeDraged = true;
    [SerializeField] private Image image;
    [SerializeField] private int pocetnaVrijednost;
    public AllowedDirection allowedDirection;

    public int MyValue
    {
        get => myValue;
        set
        {
            myValue = value;
            image.sprite = gm.settings.tileSprites[myValue];
        }
    }
    private int myValue;

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
    private Vector2Int myPosition;
    private Vector2Int oppositePosition;

    private Vector2 dragDelta;
    private Vector2Int moveDir;


    private void Awake()
    {
        gm = GameManager.Instance;
    }

    void Start()
    {
        MyValue = pocetnaVrijednost;
    }

    void OnEnable()
    {
        SquariconGlobalEvents.OnSkinUpdated += HandleSkinUpdated;
    }

    void OnDisable()
    {
        SquariconGlobalEvents.OnSkinUpdated -= HandleSkinUpdated;
    }

    void HandleSkinUpdated()
    {
        MyValue = MyValue;
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
        gm.MiddlePointActivation(new Vector2Int(1,1));
        gm.HandleMoveFinished();
    }
}
