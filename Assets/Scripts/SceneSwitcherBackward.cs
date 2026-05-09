using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcherBackward : MonoBehaviour
{
    /// Tag the collider must have to trigger the scene load. Default: "Hand"
    [SerializeField] private string triggerTag = "Hand";

    /// Loads the previous scene when a tagged collider enters the trigger zone
    /// Saves SpawnPoint = "Exit" so the destination scene spawns the player at its exit point
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(triggerTag)) return;

        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        Debug.Log("[SceneSwitcherBackward] Current build index: " + currentIndex);

        int previousIndex = currentIndex - 1;

        // Don't go below first scene in Build Settings
        if (previousIndex < 0)
        {
            Debug.LogWarning("[SceneSwitcherBackward] Already at first build scene, cannot go back!");
            return;
        }

        Debug.Log("[SceneSwitcherBackward] Loading previous index: " + previousIndex);

        // Going back, so spawn at Exit in the previous scene
        PlayerPrefs.SetString("SpawnPoint", "Exit");
        PlayerPrefs.Save();

        if (SceneController.instance != null)
        {
            SceneController.instance.LoadScene(previousIndex);
        }
        else
        {
            Debug.LogError("[SceneSwitcherBackward] SceneController.Instance is null!");
        }
    }
}