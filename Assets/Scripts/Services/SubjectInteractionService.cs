using UnityEngine;
/// <summary>
/// This service will be responsible for the Interaction(tap on the button)
/// It checks stamina and number of available interactions
/// Adds score, stamina, interactions
/// Future: Add the Event inside the function responsible doing the change
/// </summary>
public class SubjectInteractionService
{
    private readonly PlayerStateService _playerState;
    private readonly DayCycleService _dayCycleService;
    private readonly SubjectSelectionService _selectionService;
    private readonly PlayerSaveService _saveService;
    private readonly ExamService _examService;
    private readonly SubjectService _subjectService;
    
    public SubjectInteractionService(
        PlayerStateService playerState,
        DayCycleService dayCycleService,
        SubjectSelectionService selectionService,
        PlayerSaveService saveService,
        ExamService examService,
        SubjectService subjectService)
    {
        _playerState = playerState;
        _dayCycleService = dayCycleService;
        _selectionService = selectionService;
        _saveService = saveService;
        _examService = examService;
        _subjectService = subjectService;
    }
    public bool TryPerformSubject(SubjectDisplayData selection)
    {
        if (_dayCycleService.IsEndingDay)
            return false;
        
        if (_examService.IsExamActive) 
            return false;
        
        if (!_playerState.CanPerformInteraction(selection.InteractionCost))
            return false;
        
        int levelBefore = _playerState.GetSubjectLevel(selection.Subject);
        SubjectData levelData = _subjectService.GetLevelData(selection.Subject, levelBefore);
        int maxLevel = _subjectService.GetMaxLevel(selection.Subject);
        
        _playerState.ApplyStaminaChange(selection.StaminaCost, selection.StaminaRestore);
        _playerState.AddScore(selection.Subject, selection.ScoreGain);
        _playerState.UseInteractions(selection.InteractionCost);
        
        
        bool leveledUp = _playerState.RegisterSubjectInteraction(
            selection.Subject,
            levelData.interactionsToUnlockNextLevel,
            maxLevel);
        
        if (leveledUp)
            GameEvents.RaiseSubjectLevelChanged(selection.Subject, _playerState.GetSubjectLevel(selection.Subject));
        
        GameEvents.RaiseStaminaChanged(_playerState.CurrentStamina, _playerState.MaxStamina);
        GameEvents.RaiseInteractionsChanged(_playerState.InteractionsUsed, _playerState.MaxInteractions);
        GameEvents.RaiseSubjectScoreChanged(
            selection.Subject,
            _playerState.GetSubjectScore(selection.Subject));
        bool dayEnded = false;
        if (_playerState.CurrentStamina <= 0)
        {
            _dayCycleService.OnStaminaDepleted();
            dayEnded = true;
        }
        else if (_playerState.InteractionsUsed >= _playerState.MaxInteractions)
        {
            _dayCycleService.OnDayInteractionsCompleted();
            dayEnded = true;
        }
        
        if (dayEnded)
            _saveService.Save();   
        else
        {
            _saveService.Save();
            RefreshSubjectButtons();
        }
        return true;
    }
    private void RefreshSubjectButtons()
    {
        var newSubjects = _selectionService.RefreshSubjects(4);
        GameEvents.RaiseDailySubjectsReady(newSubjects);
    }
}
