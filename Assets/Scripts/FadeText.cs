using UnityEngine;
using System.Collections;

public class FadeText : MonoBehaviour
{
    public CanvasGroup canvasGroup; 

    void Start()
    {
        StartCoroutine(FadeSequence());
    }

    private IEnumerator FadeSequence()
    {
        // takes 1 sec to fade in
        for (float i = 0f; i < 1f; i += Time.deltaTime)
        {
            canvasGroup.alpha = i / 1f;
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // stays 100% visible for 2 secs
        yield return new WaitForSeconds(2f);

        // takes 1 sec to fade out
        for (float i = 1f; i > 0f; i -= Time.deltaTime)
        {
            canvasGroup.alpha = i / 1f;
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }
}