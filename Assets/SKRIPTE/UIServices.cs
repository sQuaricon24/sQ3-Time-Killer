using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class UIServices
{
    public static IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration, System.Action onComplete = null)
    {
        float time = 0;
        canvasGroup.alpha = startAlpha;
        canvasGroup.gameObject.SetActive(true);

        while (time < duration)
        {
            /*if (isPaused)
                yield return new WaitForEndOfFrame();*/

            time += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, time / duration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
        onComplete?.Invoke();
    }

    public static IEnumerator BlinkCoroutine(Image image, float fromAlpha, float toAlpha, float cycleTime)
    {
        float halfCycleTime = cycleTime / 2f; // Time to go from `fromAlpha` to `toAlpha` and back
        float elapsedTime = 0f;

        // Infinite blinking loop
        while (true)
        {
            // Fade from `fromAlpha` to `toAlpha`
            while (elapsedTime < halfCycleTime)
            {
                elapsedTime += Time.deltaTime;
                float newAlpha = Mathf.Lerp(fromAlpha, toAlpha, elapsedTime / halfCycleTime);
                SetImageAlpha(image, newAlpha);
                yield return null;
            }

            // Reset elapsed time
            elapsedTime = 0f;

            // Fade from `toAlpha` back to `fromAlpha`
            while (elapsedTime < halfCycleTime)
            {
                elapsedTime += Time.deltaTime;
                float newAlpha = Mathf.Lerp(toAlpha, fromAlpha, elapsedTime / halfCycleTime);
                SetImageAlpha(image, newAlpha);
                yield return null;
            }

            // Reset elapsed time for the next cycle
            elapsedTime = 0f;
        }
    }

    private static void SetImageAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}
