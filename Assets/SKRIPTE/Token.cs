using UnityEngine;
using UnityEngine.UI;

public class Token : MonoBehaviour
{
    private GameManager gm;
    [SerializeField] Image image;
    public int Value
    {
        get => value;
        set
        {
            this.value = value;
            image.sprite = gm.settings.tokenSprites[this.value];
        }
    }
    private int value;

    public Vector2Int MyPosition
    {
        get => myPosition;
        set
        {
            myPosition = value;
            gameObject.name = $"Token {myPosition.x} {myPosition.y}";
        }
    }
    private Vector2Int myPosition;


    private void Awake()
    {
        gm = GameManager.Instance;
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
        // why this?
        Value = Value;
    }
}
