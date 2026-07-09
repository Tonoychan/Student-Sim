using UnityEngine;

/// <summary>
/// Runs exam days: shows questions one by one and tracks correct answers.
/// </summary>
public class ExamService
{
    private static readonly GameEnums.MainSubjects[] ExamSubjects =
    {
        GameEnums.MainSubjects.Math,
        GameEnums.MainSubjects.History,
        GameEnums.MainSubjects.Science,
        GameEnums.MainSubjects.Geography,
        GameEnums.MainSubjects.Arts,
        GameEnums.MainSubjects.Computer
    };
    
    private readonly ExamDataRepository _examData;
    private readonly PlayerStateService _playerState;
    private readonly GameConfigSO _gameConfig;
    private int _currentQuestionIndex;
    private int _selectedOption;      // 0 = none, 1-4 = option
    private int _correctAnswerCount;
    private bool _isExamActive;
    
    public ExamService(ExamDataRepository examData, PlayerStateService playerState, GameConfigSO gameConfig)
    {
        _examData = examData;
        _playerState = playerState;
        _gameConfig = gameConfig;
    }
    
    public int CorrectAnswerCount => _correctAnswerCount;
    public bool IsExamActive => _isExamActive;
    
    /// <summary>True if this day number is an exam day.</summary>
    public bool IsExamDay(int day) => _gameConfig.IsExamDay(day);
    
    /// <summary>Begins the exam and shows the first question.</summary>
    public void StartExam()
    {
        _currentQuestionIndex = 0;
        _selectedOption = 0;
        _correctAnswerCount = 0;
        _isExamActive = true;
        GameEvents.RaiseExamStarted();
        ShowCurrentQuestion();
    }
    
    /// <summary>Highlights the player's chosen answer (1–4).</summary>
    public void SelectAnswer(int optionIndex) // 1-4
    {
        if (!_isExamActive) return;
        _selectedOption = optionIndex;
        GameEvents.RaiseExamAnswerSelected(optionIndex);
    }
    
    /// <summary>Checks the answer and moves to the next question or finishes.</summary>
    public void SubmitAnswer()
    {
        if (!_isExamActive || _selectedOption <= 0) return;
        var subject = ExamSubjects[_currentQuestionIndex];
        int level = _playerState.GetSubjectLevel(subject);
        if (_examData.IsAnswerCorrect(subject, level, _selectedOption))
            _correctAnswerCount++;
        _currentQuestionIndex++;
        _selectedOption = 0;
        if (_currentQuestionIndex >= ExamSubjects.Length)
            CompleteExam();
        else
            ShowCurrentQuestion();
    }
    
    private void ShowCurrentQuestion()
    {
        var subject = ExamSubjects[_currentQuestionIndex];
        int level = _playerState.GetSubjectLevel(subject);
        var q = _examData.GetQuestionFor(subject, level);
        if (q == null) { /* handle missing data */ return; }
        GameEvents.RaiseExamQuestionReady(new ExamQuestionDisplayData
        {
            Subject = subject,
            SubjectLevel = level,
            QuestionIndex = _currentQuestionIndex + 1,
            TotalQuestions = ExamSubjects.Length,
            QuestionText = q.Question,
            Options = q.AnswersOptions,
        });
    }
    
    private void CompleteExam()
    {
        _isExamActive = false;
        GameEvents.RaiseExamCompleted(_correctAnswerCount);
        // DayCycleService listens and ends the exam day
    }
}

/// <summary>Data passed to the exam UI for one question.</summary>
[System.Serializable]
public class ExamQuestionDisplayData
{
    public GameEnums.MainSubjects Subject;
    public int SubjectLevel;
    public int QuestionIndex;   // 1-6 for UI "Question 2/6"
    public int TotalQuestions;
    public string QuestionText;
    public string[] Options;    // length 4
}
