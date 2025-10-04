using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChange : MonoBehaviour
{
    // public Material OriginalColor;   
    // public Material Red; 
    
    // private Renderer rend;  // rend has been used to optimized the performance
    
    void Start()
    {
        Debug.Log("Color changing script is working");

    }
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("The platform is triggered");
        if (other.CompareTag("Hand"))
        {
            Debug.Log("Player is on the platform");
        } else {
            Debug.Log("Object is not player");
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        Debug.Log("Exiting...");

    }
}