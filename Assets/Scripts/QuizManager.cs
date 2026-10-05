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

    // --- Set up in the Inspector ---
    [SerializeField] private QuizQuestion[] questions;     // question files, in quiz order
    [SerializeField] private GameObject[] questionGroups;  // pad rows (Question 1-6), in walking order; row i belongs to question i
    [SerializeField] private TMP_Text questionText;        // the sky text that displays the current question
    [SerializeField] private Collider playerBody;          // the XR Origin's CharacterController: the only thing that can answer
    [SerializeField] private XROrigin xrOrigin;            // the player rig, used to move them
    [SerializeField] private Transform startPoint;         // where to send players who fail question 1

    // --- Tracked by code while the game runs ---
    private int currentQuestion;   // which question (and row) is active, counting from 0
    private QuizState state;       // what the quiz is doing right now
    private float ignorePadsUntil; // pads are ignored until this time (stops one wrong answer firing many times)

    void Start()
    {
        // Tell all 24 pads to report to this manager.
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

        // Only the current row is ever visible.
        HideAllPads();

        // Pad i shows answer i and remembers slot number i, so it can be judged later.
        for (int i = 0; i < 4; i++)
        {
            pads[i].SetAnswer(q.answers[i], i);
        }
    }

    // Hides the labels on every pad in every row.
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
        Answerpad[] pads = questionGroups[row].GetComponentsInChildren<Answerpad>();

        Vector3 sum = Vector3.zero;
        foreach (Answerpad pad in pads)
        {
            sum += pad.transform.position;
        }

        return sum / pads.Length;
    }

    // Moves the player to 'currentRow' on the pond, facing toward 'nextRow'.
    // (For question 1, 'currentRow' is the start point, not a row.)
    void MovePlayerTo(Vector3 currentRow, Vector3 nextRow)
    {
        // The CharacterController fights direct moves, so switch it off during the move.
        playerBody.enabled = false;

        // Face from the landing spot toward the row to retry, kept level.
        Vector3 facing = nextRow - currentRow;
        facing.y = 0;
        xrOrigin.MatchOriginUpCameraForward(Vector3.up, facing);

        // Move the rig so the player's head ends up above the landing spot.
        // Done in world space: measure how far the head sits from the rig's base (sideways only),
        // then place the rig so that offset lands on the target. The rig keeps its current height.
        Transform rig = xrOrigin.Origin.transform;
        Vector3 headOffset = xrOrigin.Camera.transform.position - rig.position;
        headOffset.y = 0;
        rig.position = new Vector3(currentRow.x - headOffset.x, rig.position.y, currentRow.z - headOffset.z);

        playerBody.enabled = true;

        // TEMPORARY test log: remove once the teleport bug is solved.
        Debug.Log("Head after move: " + Vector3.Distance(startPoint.position, xrOrigin.Camera.transform.position) + " m from start");
    }

    // Called by an Answerpad whenever anything enters its trigger.
    public void PadEntered(Answerpad pad, Collider other)
    {
        // Ignore anything that isn't the player's body (hands, animals, objects).
        if (other != playerBody) return;

        // Ignore pads briefly after a wrong answer: switching the CharacterController off and on
        // makes Unity report the player "entering" the pad again, which would repeat the answer.
        if (Time.time < ignorePadsUntil) return;

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
            // The question stays the same, so they retry it.
            Vector3 landing;

            if (currentQuestion == 0)
            {
                landing = startPoint.position;
            }
            else
            {
                landing = RowCenter(currentQuestion - 1);
            }

            // TEMPORARY test log: remove once the teleport bug is solved.
            Debug.Log("Wrong on Q" + (currentQuestion + 1) + ": landing " + Vector3.Distance(startPoint.position, landing) + " m from start");

            ignorePadsUntil = Time.time + 1f;   // 1 second of ignoring pads
            MovePlayerTo(landing, RowCenter(currentQuestion));
        }
    }
}
