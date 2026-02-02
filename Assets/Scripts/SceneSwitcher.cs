using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneSwitcher : MonoBehaviour
{
    // Start is called before the first frame update
    public void EndGame(bool success)
    {

        // Change the number of the .LoadScene(0) accordingly on the Scenes in Build settings
        SceneManager.LoadScene(1);
    }

}
