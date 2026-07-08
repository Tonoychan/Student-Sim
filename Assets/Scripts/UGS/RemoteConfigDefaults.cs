/// <summary>
/// Local fallback values when Remote Config is unavailable or a key is missing.
/// These should match your current hardcoded gameplay values.
/// </summary>
public static class RemoteConfigDefaults
{
    public const int MaxStamina = 100;
    public const int MaxInteractions = 12;
    public const int DailySubjectCount = 4;
    public const float ExamScoreMultiplier = 0.5f;

    // Matches TermScheduleBuilder — used only when RC key missing
    public static int[] GetExamDays(int maxDays)
        => TermScheduleBuilder.BuildExamDays(maxDays);
}