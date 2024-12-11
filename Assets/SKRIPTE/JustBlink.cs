using System.Collections;
using UnityEngine;


public class JustBlink : MonoBehaviour
{
    [SerializeField] private float blinkCycleSpeed = 2f;
    [SerializeField] private CanvasGroup myCanvasGroup;

    private float halfCycleDuration;
    private Coroutine blinkCoroutine;

    private void Awake()
    {
        halfCycleDuration = (float)blinkCycleSpeed / 2f;
    }

    private void OnEnable()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        blinkCoroutine = StartCoroutine(BlinkCoroutine());
    }

    private void OnDisable()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
    }

    // Blink infanajtly
    private IEnumerator BlinkCoroutine()
    {
        while (true)
        {
            yield return UIServices.FadeCanvasGroup(myCanvasGroup, 1, 0, halfCycleDuration);
            yield return UIServices.FadeCanvasGroup(myCanvasGroup, 0, 1, halfCycleDuration);
        }
    }
}
