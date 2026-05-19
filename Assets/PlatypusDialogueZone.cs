using System.Collections;
using UnityEngine;
using TMPro;

public class PlatypusDialogueZone : MonoBehaviour
{
    [TextArea(3, 8)]
    public string dialogueText;

    public GameObject dialogueBox;
    public TMP_Text dialogueLabel;

    public float typingSpeed = 0.04f;

    private Coroutine typingCoroutine;

    void Start()
    {
        if (dialogueBox != null)
            dialogueBox.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            dialogueBox.SetActive(true);

            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            typingCoroutine = StartCoroutine(TypeText());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            StopDialogue();
        }
    }

    IEnumerator TypeText()
    {
        dialogueLabel.text = "";

        foreach (char letter in dialogueText)
        {
            dialogueLabel.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    public void StopDialogue()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialogueLabel.text = "";

        if (dialogueBox != null)
            dialogueBox.SetActive(false);
    }
}