using System.Collections.Generic;
using UnityEngine;

/// <summary>Final term results shown on the end-of-term screen.</summary>
[System.Serializable]
public class TermResultData
{
    public int academicBase;
    public int totalExamCorrect;
    public int maxExamCorrect;
    public float examMultiplier;
    public int finalScore;
    public int maxDays;
    public List<SubjectScoreEntry> subjectScores = new();
}

/// <summary>One subject's score in the term result breakdown.</summary>
[System.Serializable]
public class SubjectScoreEntry
{
    public GameEnums.MainSubjects subject;
    public int score;
}