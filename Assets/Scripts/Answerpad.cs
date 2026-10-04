using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Answerpad : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    public int AnswerIndex { get; private set; }
    
    public void SetAnswer(string text, int index)
    {
        AnswerIndex = index;
        label.text = text;
        label.gameObject.SetActive(true);
    }

    public void Hide()
    {
        label.gameObject.SetActive(false);
    }

    private QuizManager manager;                         

    public void Init(QuizManager quizManager)
    {
        manager = quizManager;                       
    }

    void OnTriggerEnter(Collider other)
    {
        if (manager == null) return;               
        manager.PadEntered(this, other);           
    }
}
