using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SceneSwitcherForward : MonoBehaviour
{
    /// tag the collider must have to trigger the prompt. Default: "Hand"
    [SerializeField] private string triggerTag = "Hand";

    /// UI GameObject shown when the player is in the zone. Must be inactive by default
    [SerializeField] private GameObject promptUI;

    /// Input action for the confirm button, this can be changed however you want
    [SerializeField] private InputActionReference confirmAction;

    private bool _playerInZone = false;

    /// Shows the prompt and starts listening for the confirm button
    /// when a tagged collider enters the trigger zone
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(triggerTag)) return;

        _playerInZone = true;

        if (promptUI != null)
            promptUI.SetActive(true);

        if (confirmAction != null)
        {
            confirmAction.action.Enable();
            confirmAction.action.performed += OnConfirmPressed;
        }
    }

    /// Hides the prompt and stops listening when the tagged collider leaves the zone
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(triggerTag)) return;

        _playerInZone = false;
        HidePrompt();
    }

    /// Called when the confirm button is pressed. Loads the next scene by build index
    /// and saves "Entry" as the spawn point so the destination scene places the player correctly
    private void OnConfirmPressed(InputAction.CallbackContext context)
    {
        if (!_playerInZone) return;

        HidePrompt();

        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        Debug.Log("[SceneSwitcherForward] Loading next index: " + nextIndex);

        PlayerPrefs.SetString("SpawnPoint", "Entry");
        PlayerPrefs.Save();

        if (SceneController.instance != null)
        {
            SceneController.instance.LoadScene(nextIndex);
        }
        else
        {
            Debug.LogError("[SceneSwitcherForward] SceneController.Instance is null!");
        }
    }

    /// Hides the prompt UI and unsubscribes from the confirm action
    private void HidePrompt()
    {
        if (promptUI != null)
            promptUI.SetActive(false);

        if (confirmAction != null)
            confirmAction.action.performed -= OnConfirmPressed;
    }

    /// Ensures the confirm action listener is cleaned up when the object is destroyed
    private void OnDestroy()
    {
        if (confirmAction != null)
            confirmAction.action.performed -= OnConfirmPressed;
    }
}