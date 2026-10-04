using UnityEngine;
using System.Collections;

public class EggSpeech : MonoBehaviour
{
    public GameObject speechText;        // normal speech (shown when Hand is near)
    public GameObject eggSpeechText;     // speech shown after receiving the egg
    public GameObject confettiPrefab;    // optional
    public float eggSpeechDuration = 4f;

    bool eggReceived = false;
    bool showingEggSpeech = false;

    void Start()
    {
        if (speechText != null) speechText.SetActive(false);
        if (eggSpeechText != null) eggSpeechText.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Egg") && !eggReceived)
        {
            eggReceived = true;

            if (confettiPrefab != null)
                Instantiate(
                    confettiPrefab,
                    transform.position + Vector3.up * 0.5f,   // on the platypus, slightly raised
                    confettiPrefab.transform.rotation          // keeps the prefab's upward rotation
            );

            Destroy(other.gameObject);   // only the egg is destroyed
            StartCoroutine(ShowEggSpeech());
            return;
        }

        if (other.CompareTag("Hand") && !showingEggSpeech && speechText != null)
            speechText.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hand") && speechText != null)
            speechText.SetActive(false);
    }

    IEnumerator ShowEggSpeech()
    {
        showingEggSpeech = true;
        if (speechText != null) speechText.SetActive(false);
        if (eggSpeechText != null) eggSpeechText.SetActive(true);

        yield return new WaitForSeconds(eggSpeechDuration);

        if (eggSpeechText != null) eggSpeechText.SetActive(false);
        showingEggSpeech = false;
    }
}