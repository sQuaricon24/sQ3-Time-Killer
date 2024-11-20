using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FirstCollection;
using UnityEngine.UI;

public class Token : MonoBehaviour
{
    GameManager gm;
    [SerializeField] Image image;
    public int Vrijednost
    {
        get => _vrijednost;
        set
        {
            _vrijednost = value;
            image.sprite = gm.setting.tokenSprites[_vrijednost];
        }
    }
    int _vrijednost;

    public Vector2Int Pozicija
    {
        get => _pozicija;
        set
        {
            _pozicija = value;
            gameObject.name = $"Token {_pozicija.x} {_pozicija.y}";
        }
    }
    Vector2Int _pozicija;


    private void Awake()
    {
        gm = GameManager.gm;
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


}
