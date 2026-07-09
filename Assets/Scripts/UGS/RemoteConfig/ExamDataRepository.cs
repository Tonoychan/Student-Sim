using System.Collections.Generic;

/// <summary>
/// Loads exam questions per subject from Remote Config or local MainExam asset.
/// </summary>
public class ExamDataRepository
{
    private readonly Dictionary<GameEnums.MainSubjects, QuestionsAnswerModel[]> _pools = new();

    /// <summary>Builds the repository from remote config with ScriptableObject fallback.</summary>
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
                return;
            }
        }

        if (fallback != null && fallback.Length > 0)
            repo._pools[subject] = fallback;
    }

    /// <summary>Finds the exam question matching the player's current subject level.</summary>
    public QuestionsAnswerModel GetQuestionFor(GameEnums.MainSubjects subject, int subjectLevel)
    {
        if (!_pools.TryGetValue(subject, out QuestionsAnswerModel[] pool) || pool.Length == 0)
            return null;

        foreach (var question in pool)
        {
            if (question != null && question.questionLevel == subjectLevel)
                return question;
        }
        return null;
    }

    /// <summary>Checks if the selected option index is correct.</summary>
    public bool IsAnswerCorrect(GameEnums.MainSubjects subject, int levelIndex, int selectedOption)
    {
        var q = GetQuestionFor(subject, levelIndex);
        return q != null && q.correctAnswerFromOptions == selectedOption;
    }
}
