using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Token : MonoBehaviour
{
    [SerializeField] private int tokenID;
    [SerializeField] private Image image;
    public string ImageName => image.sprite.name;
    public int ID => tokenID;

    public void FadeIn()
    {
        ScaleIn();
        return;
        Color c = image.color;
        c.a = 255;
        image.color = c;
    }

    public void FadeOut()
    {
        ScaleOut();
        return;
        Color c = image.color;
        c.a = 0;
        image.color = c;
    }

    private RectTransform targetTransform; // Assign the RectTransform of the UI element

    public void ScaleIn()
    {
        // Scale to 1 (full size)
        targetTransform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack); // You can adjust duration and easing
    }

    public void ScaleOut()
    {
        // Scale to 0 (hidden)
        targetTransform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack); // You can adjust duration and easing
    }


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
        targetTransform = image.GetComponent<RectTransform>();
        SetImageSize(targetTransform, 50f);

        /*if (SoSetting.Instance.IsAdventureMode)
        {
            SetImageSize(targetTransform, 50f);
        }
        else
        {
            SetImageSize(targetTransform, 0f);
        }*/
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
