using System.Collections.Generic;
using UnityEngine;

public class PlayerStateService
{
    public int CurrentStamina { get; private set; } = 100;
    public int MaxStamina { get; } = 100;
    public int InteractionsUsed { get; private set; }
    public int MaxInteractions { get; } = 12;
    
    private readonly Dictionary<GameEnums.MainSubjects, int> _subjectScores = new();
    private readonly Dictionary<GameEnums.MainSubjects, int> _subjectLevels = new();
    private readonly Dictionary<GameEnums.MainSubjects, int> _dailyScoreGains = new();
    
    public int GetSubjectScore(GameEnums.MainSubjects subject)
    {
        return _subjectScores.TryGetValue(subject, out int score) ? score : 0;
    }
    
    public int GetSubjectLevel(GameEnums.MainSubjects subject)
    {
        return _subjectLevels.TryGetValue(subject, out int level) ? level : 1;
    }
    
    public bool CanPerformInteraction(int interactionCost)
    {
        return InteractionsUsed + interactionCost <= MaxInteractions;
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
    
    public void ResetForNewDay()
    {
        CurrentStamina = MaxStamina;
        InteractionsUsed = 0;
        _dailyScoreGains.Clear();
        // Keep _subjectScores across days unless you want a full reset
    }
    
    public PlayerSaveData ToSaveData(int currentDay)
    {
        var data = new PlayerSaveData
        {
            currentDay = currentDay,
            currentStamina = CurrentStamina,
            interactionsUsed = InteractionsUsed,
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
    public void ApplySaveData(PlayerSaveData data)
    {
        if (data == null)
            return;
        
        CurrentStamina = data.currentStamina;
        InteractionsUsed = data.interactionsUsed;
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
}
