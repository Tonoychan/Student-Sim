using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Loads level data for one subject from Remote Config, with a local SO fallback.
/// </summary>
public class SubjectDataLoader : ISubjectDataLoader
{
    public GameEnums.MainSubjects Subject { get; }
    
    private List<SubjectData> _cachedData;
    private bool _isLoaded;
    private readonly UnityRemoteConfigService _remoteConfig;

    private SubjectsDataSingle _localFallbackData;
    
    public SubjectDataLoader(
        GameEnums.MainSubjects subject,
        SubjectsDataSingle localFallbackData,
        UnityRemoteConfigService remoteConfig)
    {
        Subject = subject;
        _localFallbackData = localFallbackData;
        _remoteConfig = remoteConfig;
    }

    /// <summary>Tries remote config first, then falls back to the ScriptableObject.</summary>
    public async UniTask LoadAsync()
    {
        string key = RemoteConfigKeys.GetSubjectKey(Subject);
        if (_remoteConfig != null && _remoteConfig.IsInitialized && !string.IsNullOrEmpty(key) && _remoteConfig.HasKey(key))
        {
            string json = _remoteConfig.GetJsonString(key);
            SubjectData[] parsed = RemoteConfigJsonHelper.TryParseSubjectLevels(json);
            if (parsed != null)
            {
                _cachedData = new List<SubjectData>(parsed);
                _isLoaded = true;
                await UniTask.Yield();
                return;
            }
        }

        _cachedData = new List<SubjectData>(_localFallbackData.Data);
        _isLoaded = true;
        await UniTask.Yield();
    }

    public IReadOnlyList<SubjectData> GetAllLevels()
    {
        EnsureLoaded();
        return _cachedData;
    }

    public SubjectData GetLevelData(int level)
    {
        EnsureLoaded();
        return _cachedData.Find(x=> x.subjectLevel == level);
    }
    
    public void EnsureLoaded()
    {
        if (!_isLoaded)
            throw new System.InvalidOperationException("Data not loaded yet.");
    }
    
    public string SubjectName => _localFallbackData.subjectName;
    public Sprite Icon => null;
}
