using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

[RequireComponent(typeof(PlayableDirector))]
public class SinkOnTeleport : MonoBehaviour
{
    [SerializeField] private XROrigin playerOrigin;
    [SerializeField] private float delay = 0.2f;
    [Tooltip("How far below the player's feet to look for the pad.")]
    [SerializeField] private float groundCheckDistance = 0.5f;
    [Tooltip("On: sinks as soon as the player steps or teleports onto it. Off: only sinks when Sink() is called (quiz answer pads, sunk by QuizManager on a wrong answer).")]
    [SerializeField] private bool sinkWhenSteppedOn = true;

    // Base class so this works for both TeleportationArea and TeleportationAnchor pads.
    private BaseTeleportationInteractable anchor;
    private PlayableDirector director;
    private CharacterController playerController;
    private Collider[] padColliders;

    private bool playerWasOnPad;
    private bool sinking;
    private bool carryingPlayer;
    private Vector3 lastPadPosition;
    private DisplayAnswer displayAnswer;   // if this pad has one and it's marked correct, it never sinks when stepped on

    void Awake()
    {
        anchor = GetComponentInChildren<BaseTeleportationInteractable>();
        director = GetComponent<PlayableDirector>();
        padColliders = GetComponentsInChildren<Collider>();
        displayAnswer = GetComponentInChildren<DisplayAnswer>();

        if (playerOrigin == null)
            playerOrigin = FindFirstObjectByType<XROrigin>();

        if (playerOrigin == null)
            Debug.LogError("SinkOnTeleport: XR Origin not found!");
        else
            playerController = playerOrigin.GetComponent<CharacterController>();

        if (padColliders.Length == 0)
            Debug.LogError($"SinkOnTeleport: no collider found on {name}!");

        Debug.Log($"SinkOnTeleport ready on {name} ({padColliders.Length} colliders, player: {(playerOrigin ? playerOrigin.name : "none")})");
    }

    void OnEnable()
    {
        if (anchor != null)
            anchor.teleporting.AddListener(OnTeleporting);
        director.stopped += OnTimelineStopped;
    }

    void OnDisable()
    {
        if (anchor != null)
            anchor.teleporting.RemoveListener(OnTeleporting);
        director.stopped -= OnTimelineStopped;
        carryingPlayer = false;
        sinking = false;
    }

    void Update()
    {
        if (playerOrigin == null || !SinksWhenSteppedOn())
            return;

        // Walking onto the pad: start sinking the moment the player steps on (not every frame they stay on).
        bool playerOnPad = IsPlayerOnPad();
        if (playerOnPad && !playerWasOnPad)
            StartSink();
        playerWasOnPad = playerOnPad;
    }

    private bool IsPlayerOnPad()
    {
        // XR Origin's position is at the player's feet
        Vector3 feet = playerOrigin.transform.position;
        if (playerController != null)
            feet = playerOrigin.transform.TransformPoint(playerController.center) + Vector3.down * (playerController.height * 0.5f);

        var ray = new Ray(feet + Vector3.up * 0.1f, Vector3.down);
        foreach (Collider padCollider in padColliders)
        {
            if (!padCollider.isTrigger && padCollider.Raycast(ray, out _, groundCheckDistance + 0.1f))
                return true;
        }
        return false;
    }

    private void OnTeleporting(TeleportingEventArgs args)
    {
        if (SinksWhenSteppedOn())
            StartSink();
    }

    // The right answer stays up; every other pad sinks when stepped on (if sinkWhenSteppedOn is ticked).
    private bool SinksWhenSteppedOn()
    {
        bool isRightAnswer = displayAnswer != null && displayAnswer.IsCorrect;
        return sinkWhenSteppedOn && !isRightAnswer;
    }

    // Sinks the pad (carrying the player if they're on it), whatever sinkWhenSteppedOn is set to.
    public void Sink() => StartSink();

    // Stops moving the player with the pad, e.g. once they've been teleported off it.
    public void StopCarrying() => carryingPlayer = false;

    private void StartSink()
    {
        if (sinking)
            return;

        Debug.Log($"SinkOnTeleport: player on {name}, sinking");
        sinking = true;
        StartCoroutine(PlayAfterDelay());
    }

    // Wait so the player has actually landed on the pad first.
    private IEnumerator PlayAfterDelay()
    {
        yield return new WaitForSeconds(delay);

        lastPadPosition = transform.position;
        carryingPlayer = playerOrigin != null && IsPlayerOnPad();

        director.time = 0;
        director.Play();
    }

    // Timeline animates the pad before LateUpdate, so apply the pad's movement to the player here.
    void LateUpdate()
    {
        if (!carryingPlayer)
            return;

        Vector3 padDelta = transform.position - lastPadPosition;
        lastPadPosition = transform.position;

        if (padDelta == Vector3.zero)
            return;

        playerOrigin.transform.position += padDelta;

        // Keep the CharacterController from snapping the player back to its old position.
        if (playerController != null)
            Physics.SyncTransforms();
    }

    private void OnTimelineStopped(PlayableDirector _)
    {
        carryingPlayer = false;
        sinking = false;
    }
}
