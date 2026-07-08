using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

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

    public async UniTask LoadAsync()
    {
        string key = RemoteConfigKeys.GetSubjectKey(Subject);
        if (_remoteConfig != null && _remoteConfig.IsInitialized && !string.IsNullOrEmpty(key) && _remoteConfig.HasKey(key))
        {
            string json = _remoteConfig.GetJsonString(key);
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning($"[RemoteConfig][DEBUG] {Subject}: key '{key}' exists but JSON is empty.");
            }
            else
            {
                Debug.Log($"[RemoteConfig][DEBUG] {Subject}: JSON length={json.Length}, starts with={json.TrimStart().Substring(0, System.Math.Min(20, json.TrimStart().Length))}");
            }
            SubjectData[] parsed = RemoteConfigJsonHelper.TryParseSubjectLevels(json);
            if (parsed != null)
            {
                _cachedData = new List<SubjectData>(parsed);
                _isLoaded = true;
                Debug.Log($"[RemoteConfig] Loaded {_cachedData.Count} level(s) for {Subject}.");
                await UniTask.Yield();
                return;
            }
        }
        //(fallback)
        _cachedData = new List<SubjectData>(_localFallbackData.Data);
        _isLoaded = true;
        Debug.Log($"[RemoteConfig] Using local SO fallback for {Subject}.");
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
