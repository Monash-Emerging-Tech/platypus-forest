using UnityEngine;

// Shows this lily pad's answer on its label, and marks whether it's the right answer.
// The right answer stays up when stepped on; the others sink (see SinkOnTeleport).
public class DisplayAnswer : MonoBehaviour
{
    [SerializeField] private string answer;
    [SerializeField] private TMPro.TextMeshPro answerText;
    [Tooltip("Tick on the pad with the right answer: it stays up. Unticked pads sink when stepped on.")]
    [SerializeField] private bool isCorrect;

    public bool IsCorrect => isCorrect;
    public string Answer => answer;

    void Start()
    {
        // Leave the label alone if no answer is typed in (e.g. the QuizManager is filling it in).
        if (answerText != null && !string.IsNullOrEmpty(answer))
            answerText.text = answer;
    }

    // Shows or hides this pad's answer label (used by QuizFirefly to reveal one row at a time).
    public void SetAnswerVisible(bool visible)
    {
        if (answerText == null) return;

        if (visible && !string.IsNullOrEmpty(answer))
            answerText.text = answer;

        answerText.gameObject.SetActive(visible);
    }
}
