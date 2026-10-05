using UnityEngine;

public class PadTriggerTest : MonoBehaviour
{
    public Collider playerBody;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"{name}: something entered -> {other.name}");
        if (other != playerBody) return;
        Debug.Log($"{name}: PLAYER entered");
    }
}
