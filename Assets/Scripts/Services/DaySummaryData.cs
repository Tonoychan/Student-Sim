using System.Collections.Generic;
using UnityEngine;

/// <summary>Data shown on the end-of-day summary popup.</summary>
[System.Serializable]
public class DaySummaryData
{
    public int completedDay;
    public string endReason;
    public List<SubjectDayGainEntry> subjectGains = new();
}

/// <summary>How much score one subject gained today.</summary>
[System.Serializable]
public class SubjectDayGainEntry
{
    public GameEnums.MainSubjects subject;
    public int scoreGainedToday;   // shown as "+8"
    public int totalScore;         // optional: "Total: 42"
}