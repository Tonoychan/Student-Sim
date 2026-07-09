using System.Collections.Generic;
using UnityEngine;

public class ExamDataRepository
{
    
    private readonly Dictionary<GameEnums.MainSubjects, QuestionsAnswerModel[]> _pools = new();

    
    public static ExamDataRepository Load(MainExam fallbackAsset, UnityRemoteConfigService remoteConfig)
    {
        var repo = new ExamDataRepository();

        LoadSubject(repo, GameEnums.MainSubjects.Math, fallbackAsset?.MathQuestions, remoteConfig);
        LoadSubject(repo, GameEnums.MainSubjects.Science, fallbackAsset?.ScienceQuestions, remoteConfig);
        LoadSubject(repo, GameEnums.MainSubjects.History, fallbackAsset?.HistoryQuestions, remoteConfig);
        LoadSubject(repo, GameEnums.MainSubjects.Geography, fallbackAsset?.GeographyQuestions, remoteConfig);
        LoadSubject(repo, GameEnums.MainSubjects.Arts, fallbackAsset?.ArtsQuestions, remoteConfig);
        LoadSubject(repo, GameEnums.MainSubjects.Computer, fallbackAsset?.ComputerQuestions, remoteConfig);

        return repo;
    }

    
    static void LoadSubject(
        ExamDataRepository repo,
        GameEnums.MainSubjects subject,
        QuestionsAnswerModel[] fallback,
        UnityRemoteConfigService remoteConfig)
    {
        string key = RemoteConfigKeys.GetExamKey(subject);

        if (remoteConfig != null && remoteConfig.IsInitialized && !string.IsNullOrEmpty(key) && remoteConfig.HasKey(key))
        {
            string json = remoteConfig.GetJsonString(key);
            QuestionsAnswerModel[] parsed = RemoteConfigJsonHelper.TryParseExamQuestions(json);
            if (parsed != null)
            {
                repo._pools[subject] = parsed;
                Debug.Log($"[RemoteConfig] Loaded {parsed.Length} exam question(s) for {subject}.");
                return;
            }
        }

        if (fallback != null && fallback.Length > 0)
        {
            repo._pools[subject] = fallback;
            Debug.Log($"[RemoteConfig] Using local MainExam fallback for {subject}.");
        }
    }

    
    public QuestionsAnswerModel GetQuestionFor(GameEnums.MainSubjects subject, int subjectLevel)
    {
        if (!_pools.TryGetValue(subject, out QuestionsAnswerModel[] pool) || pool.Length == 0)
            return null;

        foreach (var question in pool)
        {
            if (question != null && question.questionLevel == subjectLevel)
                return question;
        }

        Debug.LogWarning($"No exam question for {subject} level {subjectLevel}");
        return null;
    }

    
    public bool IsAnswerCorrect(GameEnums.MainSubjects subject, int levelIndex, int selectedOption)
    {
        var q = GetQuestionFor(subject, levelIndex);
        return q != null && q.correctAnswerFromOptions == selectedOption;
    }
}