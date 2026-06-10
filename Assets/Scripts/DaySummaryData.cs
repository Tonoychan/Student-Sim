using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DaySummaryData
{
    public int completedDay;
    public string endReason;
    public List<SubjectDayGainEntry> subjectGains = new();
}

[System.Serializable]
public class SubjectDayGainEntry
{
    public GameEnums.MainSubjects subject;
    public int scoreGainedToday;   // shown as "+8"
    public int totalScore;         // optional: "Total: 42"
}