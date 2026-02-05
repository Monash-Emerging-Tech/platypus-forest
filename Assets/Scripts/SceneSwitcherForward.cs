using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneSwitcherForward : MonoBehaviour
{
    [SerializeField]
    private SceneController _sceneController;

    private void OnTriggerEnter(Collider other)
    {

        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log("[SceneSwitcher] Current scene name: " + currentScene);

        // Expecting: Island1, Island2, etc.
        string numberPart = currentScene.Replace("Island", "");
        Debug.Log("[SceneSwitcher] Extracted number part: " + numberPart);

        int currentIndex = int.Parse(numberPart);
        int nextIndex = currentIndex + 1;

        string nextScene = "Island" + nextIndex;
        Debug.Log("[SceneSwitcher] Loading next scene: " + nextScene);

        _sceneController.LoadScene(nextScene);
    }
}
