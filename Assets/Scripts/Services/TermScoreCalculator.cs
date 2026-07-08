using System.Collections.Generic;
using UnityEngine;

public static class TermScoreCalculator
{
    static readonly GameEnums.MainSubjects[] Subjects =
    {
        GameEnums.MainSubjects.Math,
        GameEnums.MainSubjects.History,
        GameEnums.MainSubjects.Science,
        GameEnums.MainSubjects.Geography,
        GameEnums.MainSubjects.Arts,
        GameEnums.MainSubjects.Computer,
    };
    
    public static TermResultData Build(PlayerStateService player, GameConfigSO config, float examScoreMultiplier)
    {
        int academicBase = 0;
        var breakdown = new List<SubjectScoreEntry>();
        foreach (var subject in Subjects)
        {
            int score = player.GetSubjectScore(subject);
            academicBase += score;
            breakdown.Add(new SubjectScoreEntry { subject = subject, score = score });
        }
        
        int totalExamCorrect = player.TotalExamCorrect;
        int maxExamCorrect = config.MaxPossibleExamCorrect;
        float multiplier = 1f + examScoreMultiplier * totalExamCorrect / maxExamCorrect;
        int finalScore = Mathf.RoundToInt(academicBase * multiplier);
        return new TermResultData
        {
            academicBase = academicBase,
            totalExamCorrect = totalExamCorrect,
            maxExamCorrect = maxExamCorrect,
            examMultiplier = multiplier,
            finalScore = finalScore,
            subjectScores = breakdown,
            maxDays = config.maxDays,
        };
    }
}
