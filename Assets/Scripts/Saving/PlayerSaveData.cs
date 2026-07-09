using System;
using System.Collections.Generic;

/// <summary>All data saved for one term (day, scores, stamina, quests, etc.).</summary>
[Serializable]
public class PlayerSaveData
{
    public int currentDay = 1;
    public int currentStamina = 100;
    public int interactionsUsed = 0;
    public int totalExamCorrect = 0;
    public bool termCompleted = false; 
    public int maxDays = 30;
    public long lastSavedUtc;
    public List<CurrencyEntry> currencies = new();
    public QuestSaveData questProgress = new();
    public List<SubjectProgressEntry> subjectProgress = new();
}
/// <summary>Score and level for one subject inside the save file.</summary>
[Serializable]
public class SubjectProgressEntry
{
    public GameEnums.MainSubjects subject;
    public int score;
    public int currentLevel = 1;
    public int interactionsAtCurrentLevel;
}

/// <summary>One currency type and its amount in the save file.</summary>
[Serializable]
public class CurrencyEntry
{
    public GameEnums.CurrencyType type;
    public int amount;
}

/// <summary>Which quest the player is currently on.</summary>
[Serializable]
public class QuestSaveData
{
    public int activeQuestIndex;
}