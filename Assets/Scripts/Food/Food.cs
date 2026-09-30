using UnityEngine;

public class Food : MonoBehaviour
{
    private bool suitable = false;

    public bool IsSuitable()
    {
        return suitable;
    }

    public void SetSuitable(bool value)
    {
        suitable = value;
    }
}