using UnityEngine;

[CreateAssetMenu(menuName = "Quiz/Question")]
public class QuizQuestion : ScriptableObject{
    

    [TextArea]
    public string question;
   
    public string[] answers = new string[4];

    [Range (0,3)]
    public int correctIndex;

}
