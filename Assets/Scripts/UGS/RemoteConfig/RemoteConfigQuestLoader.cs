/// <summary>
/// Loads the quest list from Remote Config or a local QuestData asset.
/// </summary>
public static class RemoteConfigQuestLoader
{
    /// <summary>Returns quests from remote config, or falls back to the SO.</summary>
    public static QuestEntry[] Load(QuestData fallbackAsset, UnityRemoteConfigService remoteConfig)
    {
        if (remoteConfig != null && remoteConfig.IsInitialized && remoteConfig.HasKey(RemoteConfigKeys.QuestsConfig))
        {
            string json = remoteConfig.GetJsonString(RemoteConfigKeys.QuestsConfig);
            QuestEntry[] parsed = RemoteConfigJsonHelper.TryParseQuests(json);
            if (parsed != null)
                return parsed;
        }

        if (fallbackAsset?.quests != null && fallbackAsset.quests.Length > 0)
            return fallbackAsset.quests;

        return System.Array.Empty<QuestEntry>();
    }
}
