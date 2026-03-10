using UnityEngine;

public class HideTextAfterTime : MonoBehaviour
{
    public GameObject textObject;
    public float displayTime = 4f;

    void Start()
    {
        Invoke("HideText", displayTime);
    }

    void HideText()
    {
        textObject.SetActive(false);
    }
}