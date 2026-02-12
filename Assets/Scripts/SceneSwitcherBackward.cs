using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcherBackward : MonoBehaviour
{
    [SerializeField] private string triggerTag = "Hand"; // change if needed

    private void OnTriggerEnter(Collider other)
    {
        // Optional filter so random colliders don't trigger scene loads
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

        // We're going BACK, so spawn at Exit in the previous scene
        PlayerPrefs.SetString("SpawnPoint", "Exit");
        PlayerPrefs.Save();

        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadScene(previousIndex);
        }
        else
        {
            Debug.LogError("[SceneSwitcherBackward] SceneController.Instance is null!");
        }
    }
}