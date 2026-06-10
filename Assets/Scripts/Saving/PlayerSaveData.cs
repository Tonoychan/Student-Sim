using System;
using System.Collections.Generic;
[Serializable]
public class PlayerSaveData
{
    public int currentDay = 1;
    public int currentStamina = 100;
    public int interactionsUsed = 0;
    public List<SubjectProgressEntry> subjectProgress = new();
}
[Serializable]
public class SubjectProgressEntry
{
    public GameEnums.MainSubjects subject;
    public int score;
    public int currentLevel = 1;
}