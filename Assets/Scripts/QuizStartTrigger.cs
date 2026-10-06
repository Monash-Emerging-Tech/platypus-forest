using UnityEngine;

// Starts the lily pad quiz when the player walks or teleports into this trigger.
// Place it at the end of the sinking lily pad path, just before the first answer row.
// Needs a Collider with "Is Trigger" ticked on the same GameObject.
[RequireComponent(typeof(Collider))]
public class QuizStartTrigger : MonoBehaviour
{
    [SerializeField] private QuizManager quizManager;

    void OnTriggerEnter(Collider other)
    {
        if (quizManager == null) return;
        if (!quizManager.IsPlayer(other)) return;

        quizManager.StartQuiz();
    }
}
