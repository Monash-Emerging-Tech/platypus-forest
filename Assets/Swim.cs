using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swim : MonoBehaviour
{
     public Transform centerPoint;   
    public float radius = 2f;
    public float speed = 1f;
     private float angle;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
   void Update()
{
    if (centerPoint == null)
    {
        Debug.LogError("Swim: CenterPoint is NULL!");
        enabled = false;   
        return;
    }

    angle += speed * Time.deltaTime;

    float x = Mathf.Cos(angle) * radius;
    float z = Mathf.Sin(angle) * radius;

    transform.position = centerPoint.position + new Vector3(x, 0, z);

    Vector3 direction = new Vector3(-Mathf.Sin(angle), 0, Mathf.Cos(angle));
    transform.rotation = Quaternion.LookRotation(direction);
}
}
