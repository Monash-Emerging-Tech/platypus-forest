using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChange : MonoBehaviour
{
    public Material OriginalColor;   
    public Material Red; 
    
    private Renderer rend;  // rend has been used to optimized the performance
    
    void Start()
    {
        rend = GetComponent<Renderer>();
        
        // save the initial material
        // if (rend != null && OriginalColor != null)
        // {
        //     rend.material = OriginalColor;
        // }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // if (other.CompareTag("Player"))
        // {
        //     rend.material = Red;  // Switch to triggered material
        //     Debug.Log("Player is on the platform");
        // }
    }
    
    void OnTriggerExit(Collider other)
    {
        // if (other.CompareTag("Player"))
        // {
        //     rend.material = OriginalColor;  // Back to normal material
        //     Debug.Log("Player exit");
        // }
    }
}