using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireflyToggle : MonoBehaviour
{
    [SerializeField] private GameObject fireflyEffect;
    [SerializeField] private Swim platypusSwim; 
    
    void Start()
    {
        if (fireflyEffect == null)
        {
            fireflyEffect = transform.Find("Fx_FirelyFire_03")?.gameObject;
            
            if (fireflyEffect == null)
            {
                Debug.LogError("Fx_FireflyFire_03 not found!");
            }

        }

         if (platypusSwim == null)
        {
            Debug.LogError("Platypus Swim NOT assigned!");
        }
        else
        {
            platypusSwim.enabled = false; 
        }

    }
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("The platform is triggered");
        if (other.CompareTag("Hand"))
        {
            Debug.Log("Player is on the platform");
            if (fireflyEffect != null)
            {
                fireflyEffect.SetActive(false);
                Debug.Log("Firefly effect disabled");
            }

            if (platypusSwim != null)
            {
                platypusSwim.enabled = true;  
            }

        } else {
            Debug.Log("Object is not player");
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        Debug.Log("Exiting...");
        if (fireflyEffect != null)
        {
            fireflyEffect.SetActive(true);
            Debug.Log("Firefly effect enabled");
        }
    }
}