using UnityEngine;
using TMPro;
using Unity.XR.CoreUtils;

public class QuizManager : MonoBehaviour
{
    private enum QuizState { NotStarted, Asking, Sinking, Finished }

    [SerializeField] private QuizQuestion[] questions;     // question files, in quiz order
    [SerializeField] private GameObject[] questionGroups;  // pad rows (Question 1-6), in walking order; row i belongs to question i
    [SerializeField] private TMP_Text questionText;        // the text that displays the current question, moved above the active row
    [SerializeField] private float questionHeight = 4f;    // how far above the active row the question floats
    [SerializeField] private Collider playerBody;          // the XR Origin's CharacterController: the only thing that can answer
    [SerializeField] private XROrigin xrOrigin;            // the player rig, used to move them
    [SerializeField] private Transform startPoint;         // where to send players who fail question 1 (solid ground before the first row)
    [SerializeField] private Collider water;               // the pond (LargeIslandBase (3)): touching it sends the player back
    [Tooltip("If the player hasn't touched the water this long after a wrong pad starts sinking, send them back anyway.")]
    [SerializeField] private float sinkFallbackSeconds = 1.5f;

    private int currentQuestion;        // which question (and row) is active, counting from 0
    private QuizState state;            // what the quiz is doing right now
    private float ignorePadsUntil;      // pads and water are ignored until this time (stops one event firing many times)
    private Vector3 lastSafeSpot;       // the pad the player last answered correctly on (or the start point)
    private SinkOnTeleport sinkingPad;  // the wrong pad currently sinking, if any
    private float sinkFallbackTime;     // when to give up waiting for the water and send the player back

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

        // Nothing is shown until the player reaches the quiz.
        state = QuizState.NotStarted;
        lastSafeSpot = startPoint.position;
        questionText.text = "";
        HideAllPads();
    }

    void Update()
    {
        if (Time.time < ignorePadsUntil) return;

        // Touching the water at any point sends the player back to safety.
        if (PlayerInWater())
        {
            SendPlayerBack();
        }
        // The wrong pad has sunk but the player never reached the water: send them back anyway.
        else if (state == QuizState.Sinking && Time.time >= sinkFallbackTime)
        {
            SendPlayerBack();
        }
    }

    void LateUpdate()
    {
        // Keep the question turned toward the player (upright, only turning sideways).
        Vector3 away = questionText.transform.position - xrOrigin.Camera.transform.position;
        away.y = 0;
        if (away.sqrMagnitude > 0.001f)
            questionText.transform.rotation = Quaternion.LookRotation(away);
    }

    // Called when the player reaches the quiz. Shows question 1 on the first row in front of them.
    public void StartQuiz()
    {
        if (state != QuizState.NotStarted) return;

        ShowQuestion(0);
    }

    // True if 'other' is the player's body (used by the start trigger too).
    public bool IsPlayer(Collider other)
    {
        return other == playerBody;
    }

    // Displays question number 'index': its text above its row and its 4 answers on the pads.
    void ShowQuestion(int index)
    {
        currentQuestion = index;
        state = QuizState.Asking;   // now waiting for the player to pick a pad

        QuizQuestion q = questions[index];
        questionText.text = q.question;
        questionText.transform.position = RowCenter(index) + Vector3.up * questionHeight;

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

    // Returns the first solid collider straight below 'from' that isn't part of the player rig.
    bool SurfaceBelow(Vector3 from, out RaycastHit surface)
    {
        surface = default;
        float nearest = float.MaxValue;

        foreach (RaycastHit hit in Physics.RaycastAll(from, Vector3.down, 50f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.transform.IsChildOf(xrOrigin.transform)) continue;

            if (hit.distance < nearest)
            {
                nearest = hit.distance;
                surface = hit;
            }
        }

        return nearest < float.MaxValue;
    }

    bool PlayerInWater()
    {
        if (water == null) return false;

        Vector3 head = xrOrigin.Camera.transform.position;
        if (!SurfaceBelow(head + Vector3.up * 2f, out RaycastHit surface)) return false;
        if (surface.collider != water) return false;

        float feetHeight = xrOrigin.Origin.transform.position.y;
        return feetHeight <= surface.point.y + 0.2f;
    }

    // Moves the player onto the surface at 'landing', facing toward 'lookAt'.
    void MovePlayerTo(Vector3 landing, Vector3 lookAt)
    {
        // The CharacterController fights direct moves, so switch it off during the move.
        playerBody.enabled = false;

        // Face from the landing spot toward the row to retry, kept level.
        Vector3 facing = lookAt - landing;
        facing.y = 0;
        if (facing.sqrMagnitude > 0.001f)
            xrOrigin.MatchOriginUpCameraForward(Vector3.up, facing);

        // Stand on top of whatever is at the landing spot (the player may have sunk below it).
        float height = landing.y;
        if (SurfaceBelow(landing + Vector3.up * 5f, out RaycastHit ground))
            height = ground.point.y;

        Transform rig = xrOrigin.Origin.transform;
        Vector3 headOffset = xrOrigin.Camera.transform.position - rig.position;
        headOffset.y = 0;
        rig.position = new Vector3(landing.x - headOffset.x, height, landing.z - headOffset.z);

        playerBody.enabled = true;
    }

    void SendPlayerBack()
    {
        // Stop the sinking pad dragging the player along after the move.
        if (sinkingPad != null)
        {
            sinkingPad.StopCarrying();
            sinkingPad = null;
        }

        if (state == QuizState.Sinking)
            state = QuizState.Asking;

        // Look toward the row to retry (the first row if the quiz hasn't started).
        int row = state == QuizState.NotStarted ? 0 : currentQuestion;

        ignorePadsUntil = Time.time + 1f;   // 1 second of ignoring pads and water
        MovePlayerTo(lastSafeSpot, RowCenter(row));
    }

    // Called by an Answerpad whenever anything enters its trigger.
    public void PadEntered(Answerpad pad, Collider other)
    {
        // Ignore anything that isn't the player's body (hands, animals, objects).
        if (other != playerBody) return;

        if (Time.time < ignorePadsUntil) return;

        if (state != QuizState.Asking) return;

        // Ignore pads that aren't in the active row (e.g. stepping back onto an old row).
        if (!pad.transform.IsChildOf(questionGroups[currentQuestion].transform)) return;

        QuizQuestion q = questions[currentQuestion];

        // Judge: compare the pad's slot number with the question's correct slot.
        if (pad.AnswerIndex == q.correctIndex)
        {
            // The correct pad stays up. The player is standing on it now,
            // so it's where they return to if they get the next one wrong.
            lastSafeSpot = pad.transform.position;

            if (currentQuestion + 1 < questions.Length)
            {

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

            state = QuizState.Sinking;
            sinkingPad = pad.GetComponentInParent<SinkOnTeleport>();
            sinkFallbackTime = Time.time + sinkFallbackSeconds;

            if (sinkingPad != null)
                sinkingPad.Sink();
            else
                SendPlayerBack();   // pad can't sink: send them back straight away
        }
    }
}
