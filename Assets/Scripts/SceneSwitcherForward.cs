using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        // Get current scene build index
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        Debug.Log("[SceneSwitcher] Current build index: " + currentIndex);

        // Calculate next index
        int nextIndex = currentIndex + 1;
        Debug.Log("[SceneSwitcher] Loading next index: " + nextIndex);

        // Save spawn direction
        PlayerPrefs.SetString("SpawnPoint", "Entry");
        PlayerPrefs.Save();

        // Load via SceneController
        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadScene(nextIndex);
        }
        else
        {
            Debug.LogError("[SceneSwitcher] SceneController.Instance is null!");
        }
    }
}