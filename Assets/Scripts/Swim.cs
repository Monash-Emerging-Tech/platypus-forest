using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swim : MonoBehaviour
{
    public Transform centerPoint;
    public float radius = 2f;  // overall scale of the infinity
    public float speed = 1f;   // movement speed
    private float angle;

    void Update()
    {
        if (centerPoint == null)
        {
            Debug.LogError("Swim: CenterPoint is NULL!");
            enabled = false;
            return;
        }

        angle += speed * Time.deltaTime;

        // Figure-eight parametric equations
        float x = radius * Mathf.Sin(angle);
        float z = radius * Mathf.Sin(angle) * Mathf.Cos(angle);

        transform.position = centerPoint.position + new Vector3(x, 0, z);

        // Calculate direction for rotation
        Vector3 direction = new Vector3(Mathf.Cos(angle) - Mathf.Sin(angle) * Mathf.Sin(angle),
                                        0,
                                        Mathf.Cos(2 * angle)); // approximate tangent
        transform.rotation = Quaternion.LookRotation(direction);
    }
}