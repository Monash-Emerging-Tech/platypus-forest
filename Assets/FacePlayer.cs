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
        Vector3 direction = vrCamera.position - transform.position;
        direction.y = 0;
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
