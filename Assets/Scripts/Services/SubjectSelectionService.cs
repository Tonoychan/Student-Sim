using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SubjectSelectionService
{
    private readonly SubjectService _subjectService;
    private readonly PlayerStateService _playerState;
    
    private static readonly GameEnums.MainSubjects[] StudyPool =
    {
        GameEnums.MainSubjects.Math,
        GameEnums.MainSubjects.History,
        GameEnums.MainSubjects.Science,
        GameEnums.MainSubjects.Geography,
        GameEnums.MainSubjects.Arts,
        GameEnums.MainSubjects.Computer,
        GameEnums.MainSubjects.Rest,
        GameEnums.MainSubjects.Exercise,
        GameEnums.MainSubjects.Work,
    };
    
    public SubjectSelectionService(SubjectService subjectService, PlayerStateService playerState)
    {
        _subjectService = subjectService;
        _playerState = playerState;
    }
    
    public IReadOnlyList<SubjectDisplayData> PickSubjects(int count = 4)
    {
        var pickedSubjects = StudyPool
            .Where(s => _subjectService.HasSubject(s))
            .OrderBy(_ => Random.value)
            .Take(count);
        return pickedSubjects
            .Select(BuildDisplayData)
            .Where(d => d != null)
            .ToList();
    }
    
    private SubjectDisplayData BuildDisplayData(GameEnums.MainSubjects subject)
    {
        // Later: read from save/progress service
        int currentLevel = GetCurrentLevelForSubject(subject);
        SubjectData levelData = _subjectService.GetLevelData(subject, currentLevel);
        
        if (levelData == null)
        {
            Debug.LogError($"Missing level {currentLevel} data for {subject}");
            return null; // filter nulls in PickSubjects
        }
        
        return new SubjectDisplayData
        {
            Subject = subject,
            DisplayName = _subjectService.GetSubjectName(subject), // see below
            CurrentLevel = currentLevel,
            classNameDescription = levelData.subjectClassName,
            levelDescription = $"Level {levelData.subjectLevel}",
            staminaDescription = $"Stamina -{levelData.staminaDeducted}",
            scoreDescription = $"Score +{levelData.subjectScore}",
            Icon = _subjectService.GetSubjectIcon(subject), // optional, separate config
            
            //Data for calculation
            StaminaCost = levelData.staminaDeducted,
            ScoreGain = levelData.subjectScore,
            InteractionCost = levelData.interactionDeducted,
            StaminaRestore = levelData.staminaRestored,
        };
    }
    private int GetCurrentLevelForSubject(GameEnums.MainSubjects subject)
    {
        return _playerState.GetSubjectLevel(subject);
    }
    
    public IReadOnlyList<SubjectDisplayData> RefreshSubjects(int count = 4)
    {
        return PickSubjects(count); // same logic, clearer name
    }
}
