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

    [Tooltip("The firefly flies here once the player stands on this pad (the StandingLilypad of the row before). " +
             "Leave empty on the first stop, so it's asked as soon as the game starts.")]
    public Transform unlockedByPad;
}

// Put this on the firefly in Island 4. It asks one question per row:
// the spawn point asks row 1, row 1's StandingLilypad asks row 2, and so on.
// The question stays up until the player stands on the next StandingLilypad.
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

    private int current = -1;          // which stop the firefly is on (-1 = not started)
    private bool flying;               // true while travelling to the current stop
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
    }

    void Update()
    {
        // Move on when the player reaches the next stop's pad.
        int next = current + 1;
        if (next < stops.Length && IsUnlocked(stops[next]))
            GoTo(next);

        if (flying) Fly();
        else Hover();
    }

    bool IsUnlocked(QuizFireflyStop stop)
    {
        return stop.unlockedByPad == null || PlayerIsStandingOn(stop.unlockedByPad);
    }

    void GoTo(int index)
    {
        current = index;
        flying = true;
        if (dialogueBox != null) dialogueBox.Hide();

        // No hover point: say it right where it is.
        if (stops[index].hoverPoint == null)
            Arrive();
    }

    void Fly()
    {
        Vector3 toTarget = stops[current].hoverPoint.position - transform.position;

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

        if (dialogueBox != null && !string.IsNullOrEmpty(stops[current].message))
            dialogueBox.ShowMessage(stops[current].message);
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
