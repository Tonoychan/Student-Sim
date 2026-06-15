using UnityEngine;

[CreateAssetMenu(fileName = "MainExam", menuName = "Advance Flow/MainExam")]
public class MainExam : ScriptableObject
{
    public QuestionsAnswerModel[] MathQuestions;
    public QuestionsAnswerModel[] ScienceQuestions;
    public QuestionsAnswerModel[] HistoryQuestions;
    public QuestionsAnswerModel[] ArtsQuestions;
    public QuestionsAnswerModel[] GeographyQuestions;
    public QuestionsAnswerModel[] ComputerQuestions;
    
    public QuestionsAnswerModel GetQuestionFor(GameEnums.MainSubjects subject, int subjectLevel)
    {
        QuestionsAnswerModel[] pool = subject switch
        {
            GameEnums.MainSubjects.Math => MathQuestions,
            GameEnums.MainSubjects.Science => ScienceQuestions,
            GameEnums.MainSubjects.History => HistoryQuestions,
            GameEnums.MainSubjects.Arts => ArtsQuestions,
            GameEnums.MainSubjects.Geography => GeographyQuestions,
            GameEnums.MainSubjects.Computer => ComputerQuestions,
            _ => null
        };
        
        if (pool == null || pool.Length == 0)
            return null;
        
        foreach (var question in pool)
        {
            if (question != null && question.questionLevel == subjectLevel)
                return question;
        }
        Debug.LogWarning($"No exam question found for {subject} at level {subjectLevel}");
        return null;
    }
    
    public bool IsAnswerCorrect(GameEnums.MainSubjects subject, int levelIndex, int selectedOption)
    {
        var q = GetQuestionFor(subject, levelIndex);
        return q != null && q.correctAnswerFromOptions == selectedOption;
    }
}
[System.Serializable]
public class QuestionsAnswerModel
{
    public GameEnums.MainSubjects subject;
    public string Question;
    public string[] AnswersOptions;
    public int correctAnswerFromOptions;
    public int questionLevel;
}

