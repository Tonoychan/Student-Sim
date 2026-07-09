using UnityEngine;

/// <summary>
/// Snapshot of gameplay tuning values applied at term start.
/// Built once from Remote Config + defaults.
/// </summary>
public class GameplaySettings
{
    public int MaxStamina { get; }
    public int MaxInteractions { get; }
    public int DailySubjectCount { get; }
    
    public float ExamScoreMultiplier { get; }

    public GameplaySettings(int maxStamina, int maxInteractions, int dailySubjectCount, float examScoreMultiplier)
    {
        MaxStamina = Mathf.Max(1, maxStamina);
        MaxInteractions = Mathf.Max(1, maxInteractions);
        DailySubjectCount = Mathf.Max(1, dailySubjectCount);
        ExamScoreMultiplier = Mathf.Max(0f, examScoreMultiplier);
    }

    /// <summary>
    /// Reads from Remote Config if ready; otherwise uses RemoteConfigDefaults.
    /// </summary>
    public static GameplaySettings FromRemoteConfig(UnityRemoteConfigService remoteConfig)
    {
        if (remoteConfig == null || !remoteConfig.IsInitialized)
            return FromDefaults();

        return new GameplaySettings(
            remoteConfig.GetInt(RemoteConfigKeys.MaxStamina, RemoteConfigDefaults.MaxStamina),
            remoteConfig.GetInt(RemoteConfigKeys.MaxInteractions, RemoteConfigDefaults.MaxInteractions),
            remoteConfig.GetInt(RemoteConfigKeys.DailySubjectCount, RemoteConfigDefaults.DailySubjectCount),
            remoteConfig.GetFloat(RemoteConfigKeys.ExamScoreMultiplier, RemoteConfigDefaults.ExamScoreMultiplier));
    }

    public static GameplaySettings FromDefaults()
    {
        return new GameplaySettings(
            RemoteConfigDefaults.MaxStamina,
            RemoteConfigDefaults.MaxInteractions,
            RemoteConfigDefaults.DailySubjectCount,
            RemoteConfigDefaults.ExamScoreMultiplier);
    }
}