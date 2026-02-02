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
        SceneManager.LoadScene(0);
    }

}
