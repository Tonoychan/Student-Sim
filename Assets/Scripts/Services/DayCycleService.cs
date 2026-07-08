using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
/// <summary>
/// Day Cycle service will be responsible checking if the current day has ended
/// then communicate with other responsibilities like UI , Helper Func
/// </summary>
public class DayCycleService
{
    private readonly GameConfigSO _gameConfig;
    static readonly TermLeaderboardService _leaderboard = new();
    private readonly PlayerStateService _playerState;
    private readonly SubjectSelectionService _selectionService;
    private readonly ExamService _examService;
    private readonly QuestService _questService;
    private readonly int _dailySubjectCount;
    private readonly float _examScoreMultiplier;
    
    private bool _isEndingDay;
    public int CurrentDay { get; private set; } = 1;
    
    public bool IsEndingDay => _isEndingDay;
    public DayCycleService(
        PlayerStateService playerState,
        SubjectSelectionService selectionService,
        ExamService examService,
        QuestService questService,
        GameConfigSO gameConfig,
        int dailySubjectCount,
        float examScoreMultiplier)
    {
        _playerState = playerState;
        _selectionService = selectionService;
        _examService = examService;
        _questService = questService;
        _gameConfig = gameConfig;
        _dailySubjectCount = dailySubjectCount;
        _examScoreMultiplier = examScoreMultiplier;
    }
    /// <summary>
    /// Called when player uses all daily interactions (12/12).
    /// </summary>
    public void OnDayInteractionsCompleted()
    {
        EndDay("Interactions completed");
    }
    /// <summary>
    /// Called when stamina reaches 0.
    /// </summary>
    public void OnStaminaDepleted()
    {
        EndDay("Stamina depleted");
    }
    /// <summary>
    /// Starts the current day: reset daily stats and show 4 subjects.
    /// </summary>
    public void StartDay(bool resetDailyStats = true)
    {
        if (resetDailyStats)
            _playerState.ResetForNewDay();
        
        GameEvents.RaiseDayChanged(CurrentDay);

        if (_examService.IsExamDay(CurrentDay))
        {
            GameEvents.RaiseDailySubjectsReady(Array.Empty<SubjectDisplayData>()); // hide subject buttons
            _examService.StartExam();
            return;
        }
        
        var subjectsToDisplay = _selectionService.PickSubjects(_dailySubjectCount);
        GameEvents.RaiseDayChanged(CurrentDay);
        GameEvents.RaiseDailySubjectsReady(subjectsToDisplay);
        GameEvents.RaiseStaminaChanged(_playerState.CurrentStamina, _playerState.MaxStamina);
        GameEvents.RaiseInteractionsChanged(_playerState.InteractionsUsed, _playerState.MaxInteractions);
    }
    private void EndDay(string reason)
    {
        // Prevent double-end if stamina hits 0 on the last interaction
        if (_isEndingDay)
            return;
        _isEndingDay = true;
        
        _questService.EvaluateActiveQuestAtDayEnd(CurrentDay);
       
        Debug.Log($"Day {CurrentDay} ended: {reason}");
        GameEvents.RaiseDayEnded();
        
        DaySummaryData summary = _playerState.BuildDaySummary(CurrentDay,reason);
        GameEvents.RaiseDaySummaryReady(summary);
    }
    
    public void ContinueToNextDay()
    {
        if (_gameConfig.IsFinalDay(CurrentDay))
            return;
        
        CurrentDay++;
        StartDay(resetDailyStats: true);
        _isEndingDay = false;
        GameEvents.RaiseDaySummaryClosed();
    }
    
    public void SetCurrentDay(int day)
    {
        CurrentDay = day;
    }
    
    public void CompleteExamDay()
    {
        GameEvents.RaiseExamClosed();
        _questService.EvaluateActiveQuestAtDayEnd(CurrentDay);
        CurrentDay++;
        StartDay(resetDailyStats: true);
    }
    
    public void CompleteTerm(PlayerStateService playerState)
    {
        GameEvents.RaiseExamClosed();
        _questService.EvaluateActiveQuestAtDayEnd(CurrentDay);
        TermResultData result = TermScoreCalculator.Build(playerState, _gameConfig, _examScoreMultiplier);
        result.maxDays = _gameConfig.maxDays;
        playerState.SetTermCompleted(true);
        GameEvents.RaiseTermResultsReady(result);
        SubmitToLeaderboardAsync(result).Forget();
    }
    
    async UniTaskVoid SubmitToLeaderboardAsync(TermResultData result)
    {
        if (!TermLeaderboardIds.HasLeaderboard(result.maxDays))
            return;
        await _leaderboard.SubmitScoreAsync(result.maxDays, result.finalScore);
    }
}
