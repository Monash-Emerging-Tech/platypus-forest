using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcherBackward : MonoBehaviour
{
    [SerializeField]
    private SceneController _sceneController;

    private void OnTriggerEnter(Collider other)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log("[SceneSwitcherBackward] Current scene name: " + currentScene);

        // Expecting: Island1, Island2, etc.
        string numberPart = currentScene.Replace("Island", "");
        Debug.Log("[SceneSwitcherBackward] Extracted number part: " + numberPart);

        int currentIndex = int.Parse(numberPart);
        int previousIndex = currentIndex - 1;

        // Don't go below Island1
        if (previousIndex < 1)
        {
            Debug.LogWarning("[SceneSwitcherBackward] Already at Island1, cannot go back!");
            return;
        }

        string previousScene = "Island" + previousIndex;
        Debug.Log("[SceneSwitcherBackward] Loading previous scene: " + previousScene);

        _sceneController.LoadScene(previousScene);
    }
}