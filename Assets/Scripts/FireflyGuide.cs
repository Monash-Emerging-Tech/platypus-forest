using UnityEngine;

public class FireflyGuide : MonoBehaviour
{
    public Transform[] waypoints;
    public string[] waypointMessages; // one message per waypoint, same length as waypoints
    public DialogueBox dialogueBox;

    public float speed = 3f;
    public float wobbleAmount = 0.3f;
    public float wobbleSpeed = 2f;
    public float stopDistance = 0.3f;

    public float hoverHeight = 0.3f;
    public float hoverSpeed = 2f;
    public float hoverDuration = 2f;

    private int currentIndex = 0;
    private bool isHovering = false;
    private float hoverTimer = 0f;
    private Vector3 hoverBasePosition;

    void Update()
    {
        if (waypoints.Length == 0) return;

        if (isHovering)
        {
            Hover();

            hoverTimer += Time.deltaTime;
            if (hoverTimer >= hoverDuration)
            {
                currentIndex++;
                isHovering = false;
                hoverTimer = 0f;
                if (dialogueBox != null) dialogueBox.Hide();
            }
            return;
        }

        if (currentIndex >= waypoints.Length) return;

        Transform target = waypoints[currentIndex];
        Vector3 toTarget = target.position - transform.position;

        if (toTarget.magnitude <= stopDistance)
        {
            isHovering = true;
            hoverBasePosition = transform.position;

            if (dialogueBox != null && currentIndex < waypointMessages.Length)
                dialogueBox.ShowMessage(waypointMessages[currentIndex]);

            return;
        }

        Vector3 direction = toTarget.normalized;
        Vector3 wobble = new Vector3(
            Mathf.Sin(Time.time * wobbleSpeed) * wobbleAmount,
            Mathf.Cos(Time.time * wobbleSpeed * 1.3f) * wobbleAmount,
            0f);

        transform.position += (direction * speed + wobble) * Time.deltaTime;
    }

    void Hover()
    {
        float yOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
        float xOffset = Mathf.Cos(Time.time * hoverSpeed * 0.7f) * hoverHeight * 0.5f;
        transform.position = hoverBasePosition + new Vector3(xOffset, yOffset, 0f);
    }
}