using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using FirstCollection;

public class Tile : MonoBehaviour, IBeginDragHandler, IDragHandler, IPointerClickHandler
{
    GameManager gm;

    [SerializeField] bool canBeDraged = true;
    [SerializeField] Image image;
    [SerializeField] int pocetnaVrijednost;
    public AllowedDirection allowedDirection;

    public int Vrijednost
    {
        get => _vrijednost;
        set
        {
            _vrijednost = value;
            image.sprite = gm.setting.tileSprites[_vrijednost];
        }
    }
    int _vrijednost;

    public Vector2Int Pozicija 
    {
        get => _pozicija;
        set
        {
            _pozicija = value;
            gameObject.name = $"Tile {_pozicija.x} {_pozicija.y}";
            _oppositePoz = new Vector2Int((_pozicija.x + 2) % 2, (_pozicija.y + 2) % 2); //pogresno, ispraviti
        }
    }
    Vector2Int _pozicija;
    Vector2Int _oppositePoz;

    Vector2 _dragDelta;
    Vector2Int _moveDir;


    private void Awake()
    {
        gm = GameManager.gm;
    }
    void Start()
    {
        Vrijednost = pocetnaVrijednost;

    }

    void OnEnable()
    {
        HelperScript.SkinUpdated += Skins;
    }
    void OnDisable()
    {
        HelperScript.SkinUpdated -= Skins;
    }
    void Skins()
    {
        Vrijednost = Vrijednost;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!canBeDraged) return;

        _dragDelta = eventData.delta;
        if (allowedDirection == AllowedDirection.MiddlePoint)
        {
            return;
        }

        if (Mathf.Abs(_dragDelta.x) > Mathf.Abs(_dragDelta.y))
        {
            if (allowedDirection == AllowedDirection.Vertical) return;

            _moveDir = new Vector2Int(_dragDelta.x > 0 ? 1 : -1, 0);
        }
        else if (Mathf.Abs(_dragDelta.x) < Mathf.Abs(_dragDelta.y))
        {
            if (allowedDirection == AllowedDirection.Horizontal) return;

            _moveDir = new Vector2Int(0, _dragDelta.y > 0 ? 1 : -1);
        }
        else return;

        gm.MoveTokenByPosition(_moveDir, Pozicija);

        Vector2Int oppositePosition = gm.OppositePos(_moveDir, Pozicija);
        
        gm.MoveTokenByPosition(-_moveDir, oppositePosition);

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
