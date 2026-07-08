using UnityEngine;

public static class RemoteConfigQuestLoader
{
    public static QuestEntry[] Load(QuestData fallbackAsset, UnityRemoteConfigService remoteConfig)
    {
        if (remoteConfig != null && remoteConfig.IsInitialized && remoteConfig.HasKey(RemoteConfigKeys.QuestsConfig))
        {
            string json = remoteConfig.GetJsonString(RemoteConfigKeys.QuestsConfig);
            QuestEntry[] parsed = RemoteConfigJsonHelper.TryParseQuests(json);
            if (parsed != null)
            {
                Debug.Log($"[RemoteConfig] Loaded {parsed.Length} quest(s) from RC.");
                return parsed;
            }
        }

        if (fallbackAsset?.quests != null && fallbackAsset.quests.Length > 0)
        {
            Debug.Log("[RemoteConfig] Using local QuestData.asset fallback.");
            return fallbackAsset.quests;
        }

        Debug.LogWarning("[RemoteConfig] No quests from RC or SO.");
        return System.Array.Empty<QuestEntry>();
    }
}