using Unity.XR.CoreUtils;
using UnityEngine;

public class WaterRespawn : MonoBehaviour
{
    [Tooltip("The trigger that counts as water (e.g. a Box Collider with Is Trigger ticked, top at the water surface). " +
             "Uses this object's own collider if left empty.")]
    [SerializeField] private Collider waterArea;
    [Tooltip("The player rig. Found automatically if left empty.")]
    [SerializeField] private XROrigin xrOrigin;
    [Tooltip("Where to respawn before the player has stood on any StandingLilypad (e.g. the shore before the first row).")]
    [SerializeField] private Transform startPoint;
    [Tooltip("Seconds to ignore the water after a respawn, so one splash only respawns once.")]
    [SerializeField] private float cooldown = 1f;
    [Tooltip("How far below the water surface the player's feet must go before respawning. Raise it if standing on a pad respawns you.")]
    [SerializeField] private float submergeDepth = 0.3f;
    [Tooltip("On: the top of Water Area is the water level for the whole scene, so going below it anywhere respawns the player. " +
             "Off: only inside the Water Area box counts.")]
    [SerializeField] private bool waterLevelEverywhere = true;

    private Collider water;
    private CharacterController playerBody;
    private Transform lastStandingPad;   // the last pad ticked Is Correct the player stood on
    private float ignoreUntil;

    void Awake()
    {
        water = waterArea != null ? waterArea : GetComponent<Collider>();

        if (water == null)
        {
            Debug.LogError("WaterRespawn: no water collider. Drag your water Box Collider into Water Area.");
            enabled = false;
            return;
        }

        if (water is MeshCollider mesh && !mesh.convex)
            Debug.LogError("WaterRespawn: tick Convex on the water's Mesh Collider, or the player can't be detected inside it.");

        if (xrOrigin == null)
            xrOrigin = FindFirstObjectByType<XROrigin>();

        if (xrOrigin == null)
            Debug.LogError("WaterRespawn: no XR Origin found in the scene.");
        else
            playerBody = xrOrigin.GetComponent<CharacterController>();
    }

    void Update()
    {
        if (xrOrigin == null || Time.time < ignoreUntil) return;

        if (SurfaceBelow(xrOrigin.Camera.transform.position + Vector3.up * 2f, out RaycastHit surface))
        {
            DisplayAnswer pad = surface.collider.GetComponentInParent<DisplayAnswer>();
            if (pad != null && pad.IsCorrect)
                lastStandingPad = pad.transform;
        }

        Vector3 feet = xrOrigin.Origin.transform.position;
        if (IsInWater(feet + Vector3.up * submergeDepth))
            Respawn();
    }

    bool IsInWater(Vector3 point)
    {

        if (water is BoxCollider box)
        {
            if (waterLevelEverywhere)
                return point.y <= box.bounds.max.y;   // below the water level, anywhere in the scene

            Vector3 local = box.transform.InverseTransformPoint(point) - box.center;
            Vector3 half = box.size * 0.5f;
            return Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.z) <= half.z && local.y <= half.y;
        }

        if (waterLevelEverywhere)
            return point.y <= water.bounds.max.y;

        if (water is MeshCollider mesh && !mesh.convex)
            return false;   // ClosestPoint only works on convex mesh colliders (error logged in Awake)

        return (water.ClosestPoint(point) - point).sqrMagnitude < 0.0001f;
    }

    void Respawn()
    {
        Transform target = lastStandingPad != null ? lastStandingPad : startPoint;
        Debug.Log($"WaterRespawn: player fell in the water, sending them to {(target != null ? target.name : "nowhere (no Start Point set)")}");
        if (target == null)
        {
            Debug.LogWarning("WaterRespawn: player touched the water, but there's no StandingLilypad visited yet and no Start Point set.");
            ignoreUntil = Time.time + cooldown;
            return;
        }

        // Stop any sinking pad from dragging the player along after the move.
        foreach (SinkOnTeleport sinking in FindObjectsByType<SinkOnTeleport>(FindObjectsSortMode.None))
            sinking.StopCarrying();

        // The CharacterController fights direct moves, so switch it off during the move.
        if (playerBody != null) playerBody.enabled = false;

        // Stand on top of the pad, with the player's head above its centre.
        Vector3 landing = target.position;
        float height = landing.y;
        if (SurfaceBelow(landing + Vector3.up * 5f, out RaycastHit ground))
            height = ground.point.y;

        Transform rig = xrOrigin.Origin.transform;
        Vector3 headOffset = xrOrigin.Camera.transform.position - rig.position;
        headOffset.y = 0;
        rig.position = new Vector3(landing.x - headOffset.x, height, landing.z - headOffset.z);

        if (playerBody != null) playerBody.enabled = true;

        ignoreUntil = Time.time + cooldown;
    }

    // The first solid collider straight below 'from' that isn't part of the player rig.
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
}
