using UnityEngine;
using TMPro;
using Unity.XR.CoreUtils;

// Runs the lily pad quiz: shows each question in the sky, puts its 4 answers
// on the matching row of pads, and judges which pad the player steps on.
// Pads report to this script; they never decide anything themselves.
public class QuizManager : MonoBehaviour
{
    // The phases the quiz can be in. Pads are only judged while Asking.
    private enum QuizState { Asking, ShowingFeedback, Finished }
    [SerializeField] private XROrigin xrOrigin;      // the player rig, used to move them
    [SerializeField] private Transform startPoint;   // where to send players who fail question 1

    // --- Set up in the Inspector ---
    [SerializeField] private QuizQuestion[] questions;     // question files, in quiz order
    [SerializeField] private GameObject[] questionGroups;  // pad rows (Question 1-6), in walking order; row i belongs to question i
    [SerializeField] private TMP_Text questionText;        // the sky text that displays the current question
    [SerializeField] private Collider playerBody;          // the XR Origin's CharacterController: the only thing that can answer

    // --- Tracked by code while the game runs ---
    private int currentQuestion;   // which question (and row) is active, counting from 0
    private QuizState state;       // what the quiz is doing right now

    void Start()
    {
        // Prepare all 24 pads: tell each one to report to this manager, and hide its label.
        foreach (GameObject group in questionGroups)
        {
            Answerpad[] pads = group.GetComponentsInChildren<Answerpad>();

            foreach (Answerpad pad in pads)
            {
                pad.Init(this);
            }
        }

        // Begin the quiz with the first question.
        ShowQuestion(0);


    }

    // Displays question number 'index': its text in the sky and its 4 answers on its row.
    void ShowQuestion(int index)
    {
        currentQuestion = index;
        state = QuizState.Asking;   // now waiting for the player to pick a pad

        QuizQuestion q = questions[index];
        questionText.text = q.question;

        Answerpad[] pads = questionGroups[index].GetComponentsInChildren<Answerpad>();

        HideAllPads();

        // Pad i shows answer i and remembers slot number i, so it can be judged later.
        for (int i = 0; i < 4; i++)
        {
            pads[i].SetAnswer(q.answers[i], i);
        }
    }

    void HideAllPads()
    {
        foreach (GameObject group in questionGroups)
        {
            Answerpad[] pads = group.GetComponentsInChildren<Answerpad>();

            foreach (Answerpad pad in pads)
            {
                pad.Hide();
            }
        }
    }

    // Returns the average position of the pads in the given row.
    Vector3 RowCenter(int row)
    {
        Answerpad[] pads = questionGroups[row].GetComponentsInChildren<Answerpad>();   // 1. which row?

        Vector3 sum = Vector3.zero;
        foreach (Answerpad pad in pads)
        {
            sum += pad.transform.position;                  // 2. add this pad's position to the total
        }

        return sum / pads.Length;                         // 3. divide by how many pads there are
    }

    // Moves the player to 'target' on the pond, facing toward 'lookAt'.
    void MovePlayerTo(Vector3 currentrow, Vector3 nextrow)
    {
        playerBody.enabled = false;                      // 1. switch the CharacterController off

        Vector3 facing = nextrow - currentrow;                 // 2. direction FROM target TO lookAt
        facing.y = 0;                                     // keep the player level
        xrOrigin.MatchOriginUpCameraForward(Vector3.up, facing);

        Vector3 head = xrOrigin.Camera.transform.position;
        xrOrigin.MoveCameraToWorldLocation(new Vector3(currentrow.x, head.y, currentrow.z));   // 3. which height?

        playerBody.enabled = true;                      // 4. switch it back on
    }



    // Called by an Answerpad whenever anything enters its trigger.
    public void PadEntered(Answerpad pad, Collider other)
    {
        // Ignore anything that isn't the player's body (hands, animals, objects).
        if (other != playerBody) return;

        // Ignore steps unless a question is waiting for an answer
        // (stops double triggers, and steps during feedback or after finishing).
        if (state != QuizState.Asking) return;

        // Ignore pads that aren't in the active row (e.g. stepping back onto an old row).
        if (!pad.transform.IsChildOf(questionGroups[currentQuestion].transform)) return;

        QuizQuestion q = questions[currentQuestion];

        // Judge: compare the pad's slot number with the question's correct slot.
        if (pad.AnswerIndex == q.correctIndex)
        {
            if (currentQuestion + 1 < questions.Length)
            {
                // Correct, and there are more questions: move to the next row.
                ShowQuestion(currentQuestion + 1);
            }
            else
            {
                // Correct on the last question: the quiz is complete.
                state = QuizState.Finished;
                questionText.text = "You made it across!";
            }
        }
        else
        {
            // Wrong answer: send the player back one row (or to the start on question 1).
            Vector3 landing;

            if (currentQuestion == 0)                 // 1. which question has no row before it?
            {
                landing = startPoint.position;               // 2. the start point's location in the world
            }
            else
            {
                landing = RowCenter(currentQuestion - 1);               // 3. the row before the current one
            }

            MovePlayerTo(landing, RowCenter(currentQuestion));      // 4. the row the player needs to retry

        }
    }
}
