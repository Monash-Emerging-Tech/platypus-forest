using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;

// One stop on the firefly's route: where it hovers, what it says, and what moves it on.
[System.Serializable]
public class QuizFireflyStop
{
    [Tooltip("Where the firefly hovers while saying this (e.g. an empty object just in front of the row).")]
    public Transform hoverPoint;

    [Tooltip("What the firefly says here (the question for the row in front of the player).")]
    [TextArea(2, 5)] public string message;

    [Tooltip("The row of pads whose answers appear with this question (e.g. the 'Question 1' row object). " +
             "Leave empty for a stop with no answers, like a final 'well done'.")]
    public Transform answersRow;

    [Tooltip("The firefly flies here once the player stands on this pad (the StandingLilypad of the row before). " +
             "Leave empty on the first stop, so it's asked as soon as the game starts.")]
    public Transform unlockedByPad;
}

public class QuizFirefly : MonoBehaviour
{
    [Tooltip("In order: row 1's question first. Add one more at the end for a 'well done' message if you like.")]
    [SerializeField] private QuizFireflyStop[] stops;
    [Tooltip("The speech bubble. Found on the firefly automatically if left empty.")]
    [SerializeField] private DialogueBox dialogueBox;
    [Tooltip("The player rig. Found automatically if left empty.")]
    [SerializeField] private XROrigin xrOrigin;

    [Header("Movement")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float wobbleAmount = 0.3f;
    [SerializeField] private float wobbleSpeed = 2f;
    [SerializeField] private float stopDistance = 0.3f;
    [SerializeField] private float hoverHeight = 0.3f;
    [SerializeField] private float hoverSpeed = 2f;

    [Header("Ending")]
    [Tooltip("The ending starts once the player stands on this (e.g. the last row's StandingLilypad, or LargeIslandBase (5) itself). " +
             "Leave empty for no ending.")]
    [SerializeField] private Transform endingUnlockedByPad;
    [Tooltip("Where the firefly flies for the goodbye, e.g. LargeIslandBase (5).")]
    [SerializeField] private Transform endingHoverPoint;
    [Tooltip("Added to the ending hover point's position, so the firefly hovers above the island instead of inside it.")]
    [SerializeField] private Vector3 endingHoverOffset = new Vector3(0f, 2f, 0f);
    [Tooltip("Said one after another once the firefly reaches the end island.")]
    [SerializeField, TextArea(1, 3)] private string[] endingMessages =
    {
        "Congratulations! You did it!",
        "Thank you for trying our little game!",
        "I hope you had a great time and learn many things!",
        "I will see you around!",
    };
    [Tooltip("How long each goodbye message stays up after it has finished typing.")]
    [SerializeField] private float secondsPerMessage = 3f;
    [Tooltip("Fades the screen to black at the end. Uses the scene's SceneFade (from the SceneController) if left empty.")]
    [SerializeField] private SceneFade screenFade;
    [SerializeField] private float fadeDuration = 2f;
    [Tooltip("How long to stay on the black screen before the game stops.")]
    [SerializeField] private float secondsOnBlack = 1f;

    private int current = -1;          // which stop the firefly is on (-1 = not started)
    private bool flying;               // true while travelling to the current stop
    private bool ending;               // true once the firefly is heading to (or at) the end island
    private Vector3 hoverBasePosition;

    void Awake()
    {
        if (dialogueBox == null)
            dialogueBox = GetComponentInChildren<DialogueBox>(true);

        if (xrOrigin == null)
            xrOrigin = FindFirstObjectByType<XROrigin>();

        // The firefly prefab also has FireflyGuide (Island 1's timed tour). Switch it off so they don't both move it.
        FireflyGuide guide = GetComponent<FireflyGuide>();
        if (guide != null) guide.enabled = false;

        hoverBasePosition = transform.position;

        // Every row's answers start hidden; each appears when the firefly asks its question.
        foreach (QuizFireflyStop stop in stops)
            SetAnswersVisible(stop.answersRow, false);

        // The fade object is switched off between fades, so include inactive objects in the search.
        if (screenFade == null)
            screenFade = FindFirstObjectByType<SceneFade>(FindObjectsInactive.Include);
    }

    void Update()
    {
        if (!ending)
        {
            // Move on when the player reaches the next stop's pad.
            int next = current + 1;
            if (next < stops.Length && IsUnlocked(stops[next]))
                GoTo(next);
            else if (next >= stops.Length && endingUnlockedByPad != null && PlayerIsStandingOn(endingUnlockedByPad))
                StartEnding();
        }

        if (flying) Fly();
        else Hover();
    }

    bool IsUnlocked(QuizFireflyStop stop)
    {
        return stop.unlockedByPad == null || PlayerIsStandingOn(stop.unlockedByPad);
    }

    void GoTo(int index)
    {
        // The row just answered is done with: hide its answers.
        if (current >= 0)
            SetAnswersVisible(stops[current].answersRow, false);

        current = index;
        flying = true;
        if (dialogueBox != null) dialogueBox.Hide();

        // No hover point: say it right where it is.
        if (stops[index].hoverPoint == null)
            Arrive();
    }

    void StartEnding()
    {
        if (current >= 0)
            SetAnswersVisible(stops[current].answersRow, false);

        ending = true;
        flying = true;
        if (dialogueBox != null) dialogueBox.Hide();

        // No hover point: say goodbye right where it is.
        if (endingHoverPoint == null)
            Arrive();
    }

    Vector3 FlyTarget()
    {
        return ending ? endingHoverPoint.position + endingHoverOffset : stops[current].hoverPoint.position;
    }

    void Fly()
    {
        Vector3 toTarget = FlyTarget() - transform.position;

        if (toTarget.magnitude <= stopDistance)
        {
            Arrive();
            return;
        }

        Vector3 wobble = new Vector3(
            Mathf.Sin(Time.time * wobbleSpeed) * wobbleAmount,
            Mathf.Cos(Time.time * wobbleSpeed * 1.3f) * wobbleAmount,
            0f);

        // Don't overshoot the stop on a slow frame.
        Vector3 step = (toTarget.normalized * speed + wobble) * Time.deltaTime;
        transform.position += Vector3.ClampMagnitude(step, toTarget.magnitude);
    }

    void Arrive()
    {
        flying = false;
        hoverBasePosition = transform.position;

        if (ending)
        {
            StartCoroutine(PlayEnding());
            return;
        }

        if (dialogueBox != null && !string.IsNullOrEmpty(stops[current].message))
            dialogueBox.ShowMessage(stops[current].message);

        // Reveal this question's answers on its row.
        SetAnswersVisible(stops[current].answersRow, true);
    }

    // Says each goodbye message in turn, then fades to black and stops the game.
    IEnumerator PlayEnding()
    {
        foreach (string message in endingMessages)
        {
            if (dialogueBox != null)
            {
                dialogueBox.ShowMessage(message);
                yield return null;   // let the typing start
                while (dialogueBox.IsTyping) yield return null;
            }

            yield return new WaitForSeconds(secondsPerMessage);
        }

        if (screenFade != null)
            yield return screenFade.FadeOutCoroutine(fadeDuration);
        else
            Debug.LogWarning("QuizFirefly: no SceneFade found, so the screen can't fade to black.");

        yield return new WaitForSeconds(secondsOnBlack);
        StopGame();
    }

    static void StopGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    static void SetAnswersVisible(Transform row, bool visible)
    {
        if (row == null) return;

        foreach (DisplayAnswer pad in row.GetComponentsInChildren<DisplayAnswer>(true))
            pad.SetAnswerVisible(visible);
    }

    void Hover()
    {
        float yOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
        float xOffset = Mathf.Cos(Time.time * hoverSpeed * 0.7f) * hoverHeight * 0.5f;
        transform.position = hoverBasePosition + new Vector3(xOffset, yOffset, 0f);
    }

    // True if the first solid thing under the player's head belongs to 'pad'.
    bool PlayerIsStandingOn(Transform pad)
    {
        if (xrOrigin == null) return false;

        Vector3 from = xrOrigin.Camera.transform.position + Vector3.up * 2f;
        float nearest = float.MaxValue;
        Collider standingOn = null;

        foreach (RaycastHit hit in Physics.RaycastAll(from, Vector3.down, 50f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.transform.IsChildOf(xrOrigin.transform)) continue;

            if (hit.distance < nearest)
            {
                nearest = hit.distance;
                standingOn = hit.collider;
            }
        }

        return standingOn != null && standingOn.transform.IsChildOf(pad);
    }
}
