using UnityEngine;

public class FacePlayer : MonoBehaviour
{
    private Transform vrCamera;

    void Start()
    {
        vrCamera = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (vrCamera == null) return;
        // TextMeshPro reads correctly from behind its +Z, so point +Z away from the player (otherwise the text is mirrored).
        Vector3 direction = transform.position - vrCamera.position;
        direction.y = 0;
        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction);
    }
}
