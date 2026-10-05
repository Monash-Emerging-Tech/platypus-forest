using UnityEngine;
using TMPro;
using Unity.XR.CoreUtils;

// Runs the lily pad quiz: shows each question in the sky, puts its 4 answers
// on the row of pads in front of the player, and judges which pad they step on.
// Pads report to this script; they never decide anything themselves.
// The quiz stays hidden until StartQuiz() is called (by a QuizStartTrigger at the
// end of the sinking lily pad path).
public class QuizManager : MonoBehaviour
{
    // The phases the quiz can be in. Pads are only judged while Asking.
    private enum QuizState { NotStarted, Asking, Finished }

    // --- Set up in the Inspector ---
    [SerializeField] private QuizQuestion[] questions;     // question files, in quiz order
    [SerializeField] private GameObject[] questionGroups;  // pad rows (Question 1-6), in walking order; row i belongs to question i
    [SerializeField] private TMP_Text questionText;        // the sky text that displays the current question
    [SerializeField] private Collider playerBody;          // the XR Origin's CharacterController: the only thing that can answer
    [SerializeField] private XROrigin xrOrigin;            // the player rig, used to move them
    [SerializeField] private Transform startPoint;         // where to send players who fail question 1 (end of the sinking path, on solid ground)

    // --- Tracked by code while the game runs ---
    private int currentQuestion;     // which question (and row) is active, counting from 0
    private QuizState state;         // what the quiz is doing right now
    private float ignorePadsUntil;   // pads are ignored until this time (stops one wrong answer firing many times)
    private Vector3 lastSafeSpot;    // the pad the player last answered correctly on (or the start point)

    void Start()
    {
        // Tell all pads to report to this manager.
        foreach (GameObject group in questionGroups)
        {
            Answerpad[] pads = group.GetComponentsInChildren<Answerpad>();

            if (pads.Length < 4)
                Debug.LogError($"QuizManager: row '{group.name}' has {pads.Length} answer pads, needs 4.");

            foreach (Answerpad pad in pads)
            {
                pad.Init(this);
            }
        }

        // Nothing is shown until the player reaches the end of the sinking path.
        state = QuizState.NotStarted;
        questionText.text = "";
        HideAllPads();
    }

    // Called when the player reaches the quiz. Shows question 1 on the first row in front of them.
    public void StartQuiz()
    {
        if (state != QuizState.NotStarted) return;

        lastSafeSpot = startPoint.position;
        ShowQuestion(0);
    }

    // True if 'other' is the player's body (used by the start trigger too).
    public bool IsPlayer(Collider other)
    {
        return other == playerBody;
    }

    // Displays question number 'index': its text in the sky and its 4 answers on its row.
    void ShowQuestion(int index)
    {
        currentQuestion = index;
        state = QuizState.Asking;   // now waiting for the player to pick a pad

        QuizQuestion q = questions[index];
        questionText.text = q.question;

        Answerpad[] pads = questionGroups[index].GetComponentsInChildren<Answerpad>();

        // Only the row in front of the player is ever visible.
        HideAllPads();

        // Pad i shows answer i and remembers slot number i, so it can be judged later.
        int count = Mathf.Min(pads.Length, q.answers.Length);
        for (int i = 0; i < count; i++)
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

    // Moves the player to 'landing', facing toward 'lookAt'.
    void MovePlayerTo(Vector3 landing, Vector3 lookAt)
    {
        // The CharacterController fights direct moves, so switch it off during the move.
        playerBody.enabled = false;

        // Face from the landing spot toward the row to retry, kept level.
        Vector3 facing = lookAt - landing;
        facing.y = 0;
        xrOrigin.MatchOriginUpCameraForward(Vector3.up, facing);

        // Move the rig so the player's head ends up above the landing spot.
        // Done in world space: measure how far the head sits from the rig's base (sideways only),
        // then place the rig so that offset lands on the target. The rig keeps its current height.
        Transform rig = xrOrigin.Origin.transform;
        Vector3 headOffset = xrOrigin.Camera.transform.position - rig.position;
        headOffset.y = 0;
        rig.position = new Vector3(landing.x - headOffset.x, rig.position.y, landing.z - headOffset.z);

        playerBody.enabled = true;
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
        // (before the quiz starts, or after finishing).
        if (state != QuizState.Asking) return;

        // Ignore pads that aren't in the active row (e.g. stepping back onto an old row).
        if (!pad.transform.IsChildOf(questionGroups[currentQuestion].transform)) return;

        QuizQuestion q = questions[currentQuestion];

        // Judge: compare the pad's slot number with the question's correct slot.
        if (pad.AnswerIndex == q.correctIndex)
        {
            // The player is now standing on this pad: it's where they return to if they get the next one wrong.
            lastSafeSpot = pad.transform.position;

            if (currentQuestion + 1 < questions.Length)
            {
                // Correct, and there are more questions: the next question's answers
                // appear on the row in front of the pad the player is standing on.
                ShowQuestion(currentQuestion + 1);
            }
            else
            {
                // Correct on the last question: the quiz is complete.
                state = QuizState.Finished;
                HideAllPads();
                questionText.text = "You made it across!";
            }
        }
        else
        {
            // Wrong answer: send the player back to the pad they last answered correctly on
            // (or the start point on question 1). The question stays the same, so they retry it.
            // Landing on a real pad, not the middle of a row, so they never drop into the water.
            ignorePadsUntil = Time.time + 1f;   // 1 second of ignoring pads
            MovePlayerTo(lastSafeSpot, RowCenter(currentQuestion));
        }
    }
}
