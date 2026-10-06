using UnityEngine;

public class CircleSwim : MonoBehaviour
{
    public float degreesPerSecond = 20f;

    void Update()
    {
        transform.Rotate(0f, degreesPerSecond * Time.deltaTime, 0f);
    }
}