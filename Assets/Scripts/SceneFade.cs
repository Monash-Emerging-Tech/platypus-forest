using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneFade : MonoBehaviour
{

    // Use image component as full screen fade overlay
    private Image _sceneFadeImage;

    private void Awake()
    {

        // Cache the image component
        _sceneFadeImage = GetComponent<Image>();
    }

    // Fades in from opaque to transparent over duration
    public IEnumerator FadeInCoroutine(float duration)
    {
        Color startColor = new Color(_sceneFadeImage.color.r, _sceneFadeImage.color.g, _sceneFadeImage.color.b, 1);
        Color targetColor = new Color(_sceneFadeImage.color.r, _sceneFadeImage.color.g, _sceneFadeImage.color.b, 0);
        
        // Smoothly transition between colors over time
        yield return (FadeCoroutine(startColor, targetColor, duration));

        // Disable the fade object after fade-in so it no longer blocks the view
        gameObject.SetActive(false);
    }

    // Fades in from opaque to transparent over duration
    public IEnumerator FadeOutCoroutine(float duration)
    {
        gameObject.SetActive(true);

        Color startColor = new Color(_sceneFadeImage.color.r, _sceneFadeImage.color.g, _sceneFadeImage.color.b, 0);
        Color targetColor = new Color(_sceneFadeImage.color.r, _sceneFadeImage.color.g, _sceneFadeImage.color.b, 1);
        
        gameObject.SetActive(true);
        yield return (FadeCoroutine(startColor, targetColor, duration));
    }

    // Core coroutine that switch between two colors over a given duration
    private IEnumerator FadeCoroutine(Color startColor, Color targetColor, float duration)
    {
        float elapsedTime = 0;
        float elapsedPercentage = 0;

        while (elapsedPercentage < 1)
        {
            elapsedPercentage = elapsedTime / duration;
            _sceneFadeImage.color = Color.Lerp(startColor, targetColor, elapsedPercentage);

            // Wait one frame before continuing
            yield return null;

            // Increment elapsed time using frame time
            elapsedTime += Time.deltaTime;
        }
    }
}
