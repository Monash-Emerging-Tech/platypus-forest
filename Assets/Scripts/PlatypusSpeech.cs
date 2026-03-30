using UnityEngine;




public class PlatypusSpeech : MonoBehaviour
{
    public GameObject speechText;

    void Start()
    {
        if (speechText != null)
        {
            speechText.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            if (speechText != null)
            {
                speechText.SetActive(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            if (speechText != null)
            {
                speechText.SetActive(false);
            }
        }
    }
}