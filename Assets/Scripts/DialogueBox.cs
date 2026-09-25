using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueBox : MonoBehaviour
{
    public TMP_Text dialogueText;
    public GameObject boxRoot; // the panel/background + text, parent object to show/hide
    public AudioSource audioSource;
    public AudioClip blipSound;
    public float charDelay = 0.03f;
    public int playBlipEveryNChars = 2; // skip some chars so it doesn't sound too rapid-fire

    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private string fullText = "";

    void Awake()
    {
        if (boxRoot != null) boxRoot.SetActive(false);
    }

    public void ShowMessage(string message)
    {
        if (boxRoot != null) boxRoot.SetActive(true);
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(message));
    }

    public void Hide()
    {
        if (boxRoot != null) boxRoot.SetActive(false);
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        isTyping = false;
    }

    IEnumerator TypeText(string message)
    {
        isTyping = true;
        fullText = message;
        dialogueText.text = "";
        int charCount = 0;

        foreach (char c in message)
        {
            dialogueText.text += c;
            charCount++;

            if (blipSound != null && audioSource != null && charCount % playBlipEveryNChars == 0 && c != ' ')
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f); // slight pitch variance, feels more alive
                audioSource.PlayOneShot(blipSound);
            }

            yield return new WaitForSeconds(charDelay);
        }

        isTyping = false;
    }


}