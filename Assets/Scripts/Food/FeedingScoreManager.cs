using UnityEngine;

public class FeedingScoreManager : MonoBehaviour
{
    public static FeedingScoreManager instance;

    private int _correctCount = 0;
    private int _totalAnswered = 0;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void RegisterAnswer(bool wasCorrect)
    {
        _totalAnswered++;
        if (wasCorrect) _correctCount++;

        Debug.Log($"[Feeding] Score: {_correctCount}/{_totalAnswered}");
    }

    public int GetCorrectCount() => _correctCount;
    public int GetTotalAnswered() => _totalAnswered;
}