using System;

/// <summary>
/// Applies term length and exam day schedule to the game config SO.
/// </summary>
public static class GameConfigLoader
{
    /// <summary>Sets max days and exam days from Remote Config or defaults.</summary>
    public static void ApplyTerm(GameConfigSO config, int maxDays)
    {
        int[] examDays = TryGetExamDaysFromRemoteConfig(maxDays)
                         ?? RemoteConfigDefaults.GetExamDays(maxDays);

        config.ApplyTerm(maxDays, examDays);
    }

    static int[] TryGetExamDaysFromRemoteConfig(int maxDays)
    {
        UnityRemoteConfigService rc = UnityRemoteConfigService.Instance;
        if (rc == null || !rc.IsInitialized)
            return null;

        string key = RemoteConfigKeys.GetExamDaysKey(maxDays);
        if (string.IsNullOrEmpty(key))
            return null;

        if (!rc.HasKey(key))
            return null;

        string raw = rc.GetString(key, string.Empty);
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        return ParseExamDays(raw, maxDays);
    }

    static int[] ParseExamDays(string commaSeparated, int maxDays)
    {
        string[] parts = commaSeparated.Split(',');
        var days = new System.Collections.Generic.List<int>();

        foreach (string part in parts)
        {
            if (!int.TryParse(part.Trim(), out int day))
                continue;

            if (day < 1 || day > maxDays)
                continue;

            if (!days.Contains(day))
                days.Add(day);
        }

        days.Sort();

        if (days.Count == 0)
            return null;

        return days.ToArray();
    }
}