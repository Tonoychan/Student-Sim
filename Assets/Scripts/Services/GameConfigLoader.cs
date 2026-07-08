using System;
using UnityEngine;

public static class GameConfigLoader
{
    public static void ApplyTerm(GameConfigSO config, int maxDays)
    {
        int[] examDays = TryGetExamDaysFromRemoteConfig(maxDays)
                         ?? RemoteConfigDefaults.GetExamDays(maxDays);

        config.ApplyTerm(maxDays, examDays);

        Debug.Log($"[GameConfig] Term {maxDays} days. Exam days: {string.Join(", ", examDays)}");
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
        {
            Debug.LogWarning($"[GameConfig] RC key '{key}' not found for {maxDays}-day term.");
            return null;
        }

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
            {
                Debug.LogWarning($"[GameConfig] Invalid exam day '{part}' in RC. Skipping.");
                continue;
            }

            if (day < 1 || day > maxDays)
            {
                Debug.LogWarning($"[GameConfig] Exam day {day} out of range 1..{maxDays}. Skipping.");
                continue;
            }

            if (!days.Contains(day))
                days.Add(day);
        }

        days.Sort();

        if (days.Count == 0)
        {
            Debug.LogWarning("[GameConfig] RC exam days parsed to empty. Using defaults.");
            return null;
        }

        return days.ToArray();
    }
}