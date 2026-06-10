using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Keeping this class as a Global Holder for Game Events to keep Organized
/// Why? Because I found this Arch better and I saw this in different other client projects
/// </summary>
public class GameEvents
{
    // -------------- DAY SUBJECT SELECTION -------------
    public static event Action<IReadOnlyList<SubjectDisplayData>> OnDailySubjectsReady;
    public static event Action<SubjectDisplayData> OnSubjectSelected;
    
    // ---------- PLAYER STATS----------------
    public static event Action<int, int> OnStaminaChanged;        
    public static event Action<int, int> OnInteractionsChanged;   
    public static event Action<GameEnums.MainSubjects, int> OnSubjectScoreChanged;
    
    // -------------- DAY CYCLE------------ ---
    public static event Action OnDayEnded;
    public static event Action<int> OnDayChanged;
    
    //---------DAY SUMMARY--------
    public static event Action<DaySummaryData> OnDaySummaryReady;
    public static event Action OnContinueToNextDay;
    public static event Action OnDaySummaryClosed;
    
    //-------------EXAMS----------------
    
    public static event Action OnExamStarted;
    public static event Action<ExamQuestionDisplayData> OnExamQuestionReady;
    public static event Action<int> OnExamAnswerSelected;   // option 1-4, for highlight
    public static event Action<int> OnExamCompleted;        // correct count, no UI required
    public static event Action OnExamClosed;
    public static event Action<int> OnExamAnswerSubmitted;
    
    
    //-------------------STATIC FUNC CALLING THE EVENTS------------------------------
    public static void RaiseDailySubjectsReady(IReadOnlyList<SubjectDisplayData> subjects)
        => OnDailySubjectsReady?.Invoke(subjects);
    public static void RaiseSubjectSelected(SubjectDisplayData selection)
        => OnSubjectSelected?.Invoke(selection);
    public static void RaiseStaminaChanged(int current, int max)
        => OnStaminaChanged?.Invoke(current, max);
    public static void RaiseInteractionsChanged(int used, int max)
        => OnInteractionsChanged?.Invoke(used, max);
    public static void RaiseSubjectScoreChanged(GameEnums.MainSubjects subject, int score)
        => OnSubjectScoreChanged?.Invoke(subject, score);
    public static void RaiseDayEnded()
        => OnDayEnded?.Invoke();
    public static void RaiseDayChanged(int day)
        => OnDayChanged?.Invoke(day);
    
    public static void RaiseDaySummaryReady(DaySummaryData summary)
        => OnDaySummaryReady?.Invoke(summary);
    public static void RaiseContinueToNextDay()
        => OnContinueToNextDay?.Invoke();
    public static void RaiseDaySummaryClosed()
        => OnDaySummaryClosed?.Invoke();
    
    public static void RaiseExamStarted() 
        => OnExamStarted?.Invoke();
    public static void RaiseExamQuestionReady(ExamQuestionDisplayData data) 
        => OnExamQuestionReady?.Invoke(data);
    public static void RaiseExamAnswerSelected(int option) 
        => OnExamAnswerSelected?.Invoke(option);
    public static void RaiseExamCompleted(int correctCount) 
        => OnExamCompleted?.Invoke(correctCount);
    
    public static void RaiseExamClosed() 
        => OnExamClosed?.Invoke();
    
    public static void RaiseExamAnswerSubmitted(int option) 
        => OnExamAnswerSubmitted?.Invoke(option);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        OnDailySubjectsReady = null;
        OnSubjectSelected = null;
        OnStaminaChanged = null;
        OnInteractionsChanged = null;
        OnSubjectScoreChanged = null;
        OnDayEnded = null;
        OnDayChanged = null;
        OnDaySummaryReady = null;
        OnContinueToNextDay = null;
        OnDaySummaryClosed = null;
    }
}
