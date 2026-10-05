using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FoodOption : MonoBehaviour
{
    [SerializeField] private bool isCorrectFood;
    [TextArea]
    [SerializeField] private string feedbackMessage;  // e.g. "Correct! Platypuses dig for worms using electroreception."

    [Header("Scene references")]
    [SerializeField] private FeedingScoreManager scoreManager;
    [SerializeField] private TextMeshPro feedbackText;  // shared text object to show result
    [SerializeField] private GameObject feedingPanel;       // panel to hide after a choiceWSSSS

    // Hook this method to the Button's OnClick() in the Inspector
    public void OnFoodSelected()
    {
        if (scoreManager != null)
            scoreManager.RegisterAnswer(isCorrectFood);
        if (feedbackText != null)
            feedbackText.text = feedbackMessage + " score: " + scoreManager.GetCorrectCount();
            if(scoreManager.GetCorrectCount()>10 && isCorrectFood)
            {
                feedbackText.text = "He's getting a bit fat isn't he?";
            }




    }

    private void ClosePanel()
    {
        if (feedingPanel != null)
            feedingPanel.SetActive(false);

        if (feedbackText != null)
            feedbackText.text = "";
    }
}