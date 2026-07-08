/// <summary>
/// Central list of Remote Config key names.
/// Keeps magic strings out of gameplay code.
/// </summary>
public static class RemoteConfigKeys
{
    // --- Gameplay (Phase 1) ---
    public const string MaxStamina = "max_stamina";
    public const string MaxInteractions = "max_interactions";
    public const string DailySubjectCount = "daily_subject_count";

    public const string ExamDays5 = "exam_days_5";
    public const string ExamDays30 = "exam_days_30";
    public const string ExamDays120 = "exam_days_120";
    public const string ExamDays360 = "exam_days_360";
    
    public const string QuestsConfig = "quests_config";
    public const string ExamScoreMultiplier = "exam_score_multiplier";
    public const string SubjectMath = "subject_math";
    public const string SubjectScience = "subject_science";
    public const string SubjectHistory = "subject_history";
    public const string SubjectGeography = "subject_geography";
    public const string SubjectArts = "subject_arts";
    public const string SubjectComputer = "subject_computer";
    public const string SubjectRest = "subject_rest";
    public const string ExamMath = "exam_math";
    public const string ExamScience = "exam_science";
    public const string ExamHistory = "exam_history";
    public const string ExamGeography = "exam_geography";
    public const string ExamArts = "exam_arts";
    public const string ExamComputer = "exam_computer";

    // --- Reserved for later phases (add keys to dashboard when ready) ---
    public const string AdStaminaReward = "ad_stamina_reward";
    public const string AdsEnabled = "ads_enabled";
    public const string AdsDailyLimit = "ads_daily_limit";
    public const string StoreCatalogJson = "store_catalog_json";

    public static string GetExamDaysKey(int maxDays)
    {
        return maxDays switch
        {
            5 => ExamDays5,
            30 => ExamDays30,
            120 => ExamDays120,
            360 => ExamDays360,
            _ => null
        };
    }
    
    public static string GetSubjectKey(GameEnums.MainSubjects subject)
    {
        return subject switch
        {
            GameEnums.MainSubjects.Math => SubjectMath,
            GameEnums.MainSubjects.Science => SubjectScience,
            GameEnums.MainSubjects.History => SubjectHistory,
            GameEnums.MainSubjects.Geography => SubjectGeography,
            GameEnums.MainSubjects.Arts => SubjectArts,
            GameEnums.MainSubjects.Computer => SubjectComputer,
            GameEnums.MainSubjects.Rest => SubjectRest,
            _ => null
        };
    }
    
    public static string GetExamKey(GameEnums.MainSubjects subject)
    {
        return subject switch
        {
            GameEnums.MainSubjects.Math => ExamMath,
            GameEnums.MainSubjects.Science => ExamScience,
            GameEnums.MainSubjects.History => ExamHistory,
            GameEnums.MainSubjects.Geography => ExamGeography,
            GameEnums.MainSubjects.Arts => ExamArts,
            GameEnums.MainSubjects.Computer => ExamComputer,
            _ => null
        };
    }
}