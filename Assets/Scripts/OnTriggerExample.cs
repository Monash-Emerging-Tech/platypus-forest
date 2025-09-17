using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnTriggerExample : MonoBehaviour
{
    public GameObject testCube;
    // Start is called before the first frame update
    void Start()
    {
        if (testCube == null)
            testCube = GameObject.FindWithTag("TestCube");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Hand")
        {
            gameObject.SetActive(false);
        }
        return;
    }
}