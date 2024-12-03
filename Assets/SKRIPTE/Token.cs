using UnityEngine;
using UnityEngine.UI;

public class Token : MonoBehaviour
{
    [SerializeField] Image image;
    public int Value
    {
        get => value;
        set
        {
            this.value = value;
            image.sprite = SoSetting.Instance.tokenSprites[this.value];
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
        if(SoSetting.Instance.IsAdventureMode)
        {
            SetImageSize(image.GetComponent<RectTransform>(), 50f);
        }
        else
        {
            SetImageSize(image.GetComponent<RectTransform>(), 0f);
        }
    }

    private void OnEnable()
    {
        SquariconGlobalEvents.OnSkinUpdated += HandleSkinUpdated;
    }

    private void OnDisable()
    {
        SquariconGlobalEvents.OnSkinUpdated -= HandleSkinUpdated;
    }

    private void HandleSkinUpdated()
    {
        // why this?
        Value = Value;
    }

    private void SetImageSize(RectTransform imageRectTransform, float value)
    {
        if (imageRectTransform == null)
        {
            Debug.LogError("Image RectTransform is null!");
            return;
        }

        imageRectTransform.offsetMin = new Vector2(value, value); // Sets left and bottom
        imageRectTransform.offsetMax = new Vector2(-value, -value); // Sets right and top
    }
}
