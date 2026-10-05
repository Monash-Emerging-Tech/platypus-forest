using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

using UnityEngine;
using UnityEngine.InputSystem;

public class FeedingGameTrigger : MonoBehaviour
{
    [SerializeField] private string triggerTag = "Hand";
    [SerializeField] private GameObject promptUI;       // "Press X to feed" prompt
    [SerializeField] private GameObject feedingPanel;    // the food-choice UI panel (inactive by default)
    [SerializeField] private InputActionReference confirmAction;

    private bool _playerInZone = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(triggerTag)) return;

        _playerInZone = true;

        if (promptUI != null)
            promptUI.SetActive(true);

        if (confirmAction != null)
            confirmAction.action.Enable();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(triggerTag)) return;

        _playerInZone = false;
        HidePrompt();
        HideFeedingPanel();
    }

    private void Update()
    {
        if (!_playerInZone || feedingPanel.activeSelf) return;
        if (confirmAction == null) return;

        if (confirmAction.action.WasPressedThisFrame())
        {
            ShowFeedingPanel();
        }
    }

    private void ShowFeedingPanel()
    {
        HidePrompt();
        if (feedingPanel != null)
            feedingPanel.SetActive(true);
    }

    public void HideFeedingPanel()
    {
        if (feedingPanel != null)
            feedingPanel.SetActive(false);
    }

    private void HidePrompt()
    {
        if (promptUI != null)
            promptUI.SetActive(false);
    }
}