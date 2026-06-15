using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TermResultData
{
    public int academicBase;
    public int totalExamCorrect;
    public int maxExamCorrect;
    public float examMultiplier;
    public int finalScore;
    public List<SubjectScoreEntry> subjectScores = new();
}

[System.Serializable]
public class SubjectScoreEntry
{
    public GameEnums.MainSubjects subject;
    public int score;
}