using UnityEngine;

[CreateAssetMenu(fileName = "MainExamData", menuName = "Scriptable Objects/MainExamData")]
public class MainExamData : ScriptableObject
{
    public QuestionsAnswers[] MathQuestions;
    public QuestionsAnswers[] ScienceQuestions;
    public QuestionsAnswers[] HistoryQuestions;
    public QuestionsAnswers[] ArtsQuestions;
    public QuestionsAnswers[] GeographyQuestions;
    public QuestionsAnswers[] ComputerQuestions;

    public QuestionsAnswers GetQuestionFor(SimulatorSubjects subject, int index)
    {
        switch (subject)
        {
            case SimulatorSubjects.Math:
                return MathQuestions[index];
            case SimulatorSubjects.Science:
                return ScienceQuestions[index];
            case SimulatorSubjects.History:
                return HistoryQuestions[index];
            case SimulatorSubjects.Arts:
                return ArtsQuestions[index];
            case SimulatorSubjects.Geography:
                return GeographyQuestions[index];
            case SimulatorSubjects.Computer:
                return ComputerQuestions[index];
            default:
                return null;
        }
    }

    public bool CheckAnswerIsCorrect(SimulatorSubjects subject,int index,int selectedAnswer)
    {
        switch (subject)
        {
            case SimulatorSubjects.Math:
                if(MathQuestions[index].correctAnswerFromOptions == selectedAnswer)
                    return true;
                break;
            case SimulatorSubjects.Science:
                if(ScienceQuestions[index].correctAnswerFromOptions == selectedAnswer)
                    return true;
                break;
            case SimulatorSubjects.History:
                if(HistoryQuestions[index].correctAnswerFromOptions == selectedAnswer)
                    return true;
                break;
            case SimulatorSubjects.Arts:
                if(ArtsQuestions[index].correctAnswerFromOptions == selectedAnswer)
                    return true;
                break;
            case SimulatorSubjects.Geography:
                if(GeographyQuestions[index].correctAnswerFromOptions == selectedAnswer)
                    return true;
                break;
            case SimulatorSubjects.Computer:
                if(ComputerQuestions[index].correctAnswerFromOptions == selectedAnswer)
                    return true;
                break;
            default:
                return false;
        }
        return false;
    }
}

[System.Serializable]
public class QuestionsAnswers
{
    public SimulatorSubjects subject;
    public string Question;
    public string[] AnswersOptions;
    public int correctAnswerFromOptions;
}
