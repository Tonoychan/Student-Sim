using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds all player stats: stamina, interactions, subject scores and levels.
/// </summary>
public class PlayerStateService
{
    public int CurrentStamina { get; private set; } = RemoteConfigDefaults.MaxStamina;
    public int MaxStamina { get; private set; } = RemoteConfigDefaults.MaxStamina;
    public int InteractionsUsed { get; private set; }
    public int MaxInteractions { get; private set; } = RemoteConfigDefaults.MaxInteractions;
    
    private readonly Dictionary<GameEnums.MainSubjects, int> _subjectScores = new();
    private readonly Dictionary<GameEnums.MainSubjects, int> _subjectLevels = new();
    private readonly Dictionary<GameEnums.MainSubjects, int> _dailyScoreGains = new();
    private readonly Dictionary<GameEnums.MainSubjects, int> _interactionsAtCurrentLevel = new();
    
    private int _totalExamCorrect;
    private bool _termCompleted;
    private int _termMaxDays = 30;
    
    public int TotalExamCorrect => _totalExamCorrect;
    public bool TermCompleted => _termCompleted;
    public int TermMaxDays => _termMaxDays;
    
    public int GetInteractionsAtCurrentLevel(GameEnums.MainSubjects subject)
        => _interactionsAtCurrentLevel.TryGetValue(subject, out int count) ? count : 0;
    
    public int GetSubjectScore(GameEnums.MainSubjects subject)
    {
        return _subjectScores.TryGetValue(subject, out int score) ? score : 0;
    }
    
    public void SetTermMaxDays(int maxDays)
    {
        _termMaxDays = maxDays;
    }
    
    public int GetSubjectLevel(GameEnums.MainSubjects subject)
    {
        return _subjectLevels.TryGetValue(subject, out int level) ? level : 1;
    }
    
    public bool CanPerformInteraction(int interactionCost)
    {
        return InteractionsUsed + interactionCost <= MaxInteractions;
    }
    
    /// <summary>Applies max stamina/interactions from Remote Config.</summary>
    public void ApplyGameplaySettings(GameplaySettings settings)
    {
        if (settings == null)
            return;
        MaxStamina = settings.MaxStamina;
        MaxInteractions = settings.MaxInteractions;
        CurrentStamina = Mathf.Clamp(CurrentStamina, 0, MaxStamina);
    }
    
    public void ApplyStaminaChange(int deducted, int restored)
    {
        CurrentStamina -= deducted;
        CurrentStamina += restored;
        CurrentStamina = Mathf.Clamp(CurrentStamina, 0, MaxStamina);
    }
    public void AddScore(GameEnums.MainSubjects subject, int amount)
    {
        if (!_subjectScores.ContainsKey(subject))
            _subjectScores[subject] = 0;
        _subjectScores[subject] += amount;
        
        if (!_dailyScoreGains.ContainsKey(subject))
            _dailyScoreGains[subject] = 0;
        _dailyScoreGains[subject] += amount;
    }
    public void UseInteractions(int count)
    {
        InteractionsUsed += count;
    }
    
    /// <summary>Resets stamina and interactions at the start of each day.</summary>
    public void ResetForNewDay()
    {
        CurrentStamina = MaxStamina;
        InteractionsUsed = 0;
        _dailyScoreGains.Clear();
        // Keep _subjectScores across days unless you want a full reset
    }
    
    /// <summary>Builds a save snapshot from current state.</summary>
    public PlayerSaveData ToSaveData(int currentDay)
    {
        var data = new PlayerSaveData
        {
            currentDay = currentDay,
            currentStamina = CurrentStamina,
            interactionsUsed = InteractionsUsed,
            totalExamCorrect = _totalExamCorrect,
            termCompleted = _termCompleted, 
            maxDays = _termMaxDays,
            subjectProgress = new List<SubjectProgressEntry>()
        };
        foreach (var subject in System.Enum.GetValues(typeof(GameEnums.MainSubjects)))
        {
            var s = (GameEnums.MainSubjects)subject;
            if (s == GameEnums.MainSubjects.None)
                continue;
            data.subjectProgress.Add(new SubjectProgressEntry
            {
                subject = s,
                score = GetSubjectScore(s),
                currentLevel = GetSubjectLevel(s)
            });
        }
        return data;
    }
    /// <summary>Restores state from a save file.</summary>
    public void ApplySaveData(PlayerSaveData data)
    {
        if (data == null)
            return;
        
        CurrentStamina = data.currentStamina;
        InteractionsUsed = data.interactionsUsed;
        _totalExamCorrect = data.totalExamCorrect;  
        _termCompleted = data.termCompleted;  
        _termMaxDays = data.maxDays > 0 ? data.maxDays : 30;
        _subjectScores.Clear();
        _subjectLevels.Clear();
        foreach (var entry in data.subjectProgress)
        {
            _subjectScores[entry.subject] = entry.score;
            _subjectLevels[entry.subject] = entry.currentLevel;
        }
    }
    
    public int GetDailyScoreGain(GameEnums.MainSubjects subject)
    {
        return _dailyScoreGains.TryGetValue(subject, out int gain) ? gain : 0;
    }
    
    private static readonly GameEnums.MainSubjects[] SummarySubjects =
    {
        GameEnums.MainSubjects.Math,
        GameEnums.MainSubjects.History,
        GameEnums.MainSubjects.Science,
        GameEnums.MainSubjects.Geography,
        GameEnums.MainSubjects.Arts,
        GameEnums.MainSubjects.Computer,
    };
    /// <summary>Builds the end-of-day summary shown to the player.</summary>
    public DaySummaryData BuildDaySummary(int completedDay, string reason)
    {
        var summary = new DaySummaryData
        {
            completedDay = completedDay,
            endReason = reason,
            subjectGains = new List<SubjectDayGainEntry>()
        };
        foreach (var subject in SummarySubjects)
        {
            summary.subjectGains.Add(new SubjectDayGainEntry
            {
                subject = subject,
                scoreGainedToday = GetDailyScoreGain(subject),
                totalScore = GetSubjectScore(subject)
            });
        }
        return summary;
    }
    
    /// <summary>Counts one subject tap toward the next level. Returns true if level went up.</summary>
    public bool RegisterSubjectInteraction(
        GameEnums.MainSubjects subject,
        int interactionsRequiredForNextLevel,
        int maxLevel)
    {
        int currentLevel = GetSubjectLevel(subject);
        if (interactionsRequiredForNextLevel <= 0 || currentLevel >= maxLevel)
            return false;
        
        int count = GetInteractionsAtCurrentLevel(subject) + 1;
        _interactionsAtCurrentLevel[subject] = count;
        if (count < interactionsRequiredForNextLevel)
            return false;
        _subjectLevels[subject] = currentLevel + 1;
        _interactionsAtCurrentLevel[subject] = 0;
        return true;
    }
    
    public void AddExamResult(int correctThisExam)
    {
        _totalExamCorrect += correctThisExam;
    }
    public void SetTermCompleted(bool completed)
    {
        _termCompleted = completed;
    }
    
    /// <summary>Clears all term progress for a new game.</summary>
    public void ResetTermProgress()
    {
        _totalExamCorrect = 0;
        _termMaxDays = 30;
        _termCompleted = false;
        _subjectScores.Clear();
        _subjectLevels.Clear();
        _interactionsAtCurrentLevel.Clear();
    }
}
