using UnityEngine;

public class FireflyTrigger : MonoBehaviour
{
    public GameObject fireflyPrefab;
    public Transform spawnPoint; // where the firefly appears, e.g. slightly above the tile
    public bool spawnOnce = true;

    [Header("Guide Path for this Firefly")]
    public Transform[] waypointsForThisFirefly;
    public string[] messagesForThisFirefly; // same length as waypointsForThisFirefly

    private bool hasSpawned = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something entered: " + other.gameObject.name + " | Tag: " + other.tag);

        if (!other.CompareTag("Hand")) return;
        if (spawnOnce && hasSpawned) return;

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        GameObject firefly = Instantiate(fireflyPrefab, pos, Quaternion.identity);

        FireflyGuide guide = firefly.GetComponent<FireflyGuide>();
        if (guide != null)
        {
            guide.waypoints = waypointsForThisFirefly;
            guide.waypointMessages = messagesForThisFirefly;
        }
        else
        {
            Debug.LogWarning("Spawned firefly has no FireflyGuide component attached.");
        }

        hasSpawned = true;

        if (spawnOnce)
        {
            gameObject.SetActive(false);
        }
    }
}