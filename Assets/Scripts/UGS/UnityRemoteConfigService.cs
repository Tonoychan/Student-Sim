using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Services.RemoteConfig;
using UnityEngine;

public class UnityRemoteConfigService
{
    public static UnityRemoteConfigService Instance { get; private set; }

    public event Action OnConfigFetched;
    public event Action<string> OnConfigFetchFailed;

    public bool IsInitialized { get; private set; }
    public bool IsFetching { get; private set; }

    private readonly Dictionary<string, object> _cache = new();

    private struct UserAttributes { }
    private struct AppAttributes { }

    public UnityRemoteConfigService()
    {
        Instance = this;
    }

    // --- Public read API ---

    public T GetValue<T>(string key, T defaultValue = default)
    {
        if (!IsInitialized)
        {
            Debug.LogWarning($"[RemoteConfig] Not initialized. Using default for '{key}'.");
            return defaultValue;
        }

        if (!_cache.TryGetValue(key, out object raw) || raw == null)
        {
            Debug.LogWarning($"[RemoteConfig] Key '{key}' missing. Using default.");
            return defaultValue;
        }

        try
        {
            if (raw is T direct)
                return direct;

            return (T)Convert.ChangeType(raw, typeof(T));
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[RemoteConfig] Cast failed for '{key}': {ex.Message}. Using default.");
            return defaultValue;
        }
    }
    
    private static bool IsJsonKey(string key)
    {
        return key.StartsWith("subject_")
               || key.StartsWith("exam_")
               || key == RemoteConfigKeys.QuestsConfig
               || key == RemoteConfigKeys.StoreCatalogJson;
    }

    public int GetInt(string key, int defaultValue)
        => GetValue(key, defaultValue);

    public bool GetBool(string key, bool defaultValue)
        => GetValue(key, defaultValue);

    public string GetString(string key, string defaultValue)
        => GetValue(key, defaultValue);

    public bool HasKey(string key)
        => IsInitialized && _cache.ContainsKey(key);

    public IReadOnlyDictionary<string, object> GetAllValues()
        => _cache;

    // --- Init / fetch ---

    public async UniTask InitRemoteConfig(string environmentId)
    {
        await InitializeAndFetchAsync(environmentId);
    }

    public async UniTask InitializeAndFetchAsync(string environmentId)
    {
        if (IsInitialized)
        {
            Debug.Log("[RemoteConfig] Already initialized. Skipping.");
            return;
        }

        if (IsFetching)
        {
            Debug.Log("[RemoteConfig] Fetch already in progress.");
            return;
        }

        IsFetching = true;

        try
        {
            RemoteConfigService.Instance.FetchCompleted += OnFetchCompleted;
            RemoteConfigService.Instance.SetEnvironmentID(environmentId);
            await RemoteConfigService.Instance.FetchConfigsAsync(
                new UserAttributes(),
                new AppAttributes());
        }
        catch (Exception ex)
        {
            IsFetching = false;
            Debug.LogError($"[RemoteConfig] Initialization/fetch failed: {ex.Message}");
            OnConfigFetchFailed?.Invoke(ex.Message);
        }
    }

    public async UniTask RefreshAsync(string environmentId)
    {
        if (IsFetching)
            return;

        IsFetching = true;
        IsInitialized = false;

        try
        {
            RemoteConfigService.Instance.FetchCompleted -= OnFetchCompleted;
            RemoteConfigService.Instance.FetchCompleted += OnFetchCompleted;
            RemoteConfigService.Instance.SetEnvironmentID(environmentId);
            await RemoteConfigService.Instance.FetchConfigsAsync(
                new UserAttributes(),
                new AppAttributes());
        }
        catch (Exception ex)
        {
            IsFetching = false;
            Debug.LogError($"[RemoteConfig] Refresh failed: {ex.Message}");
            OnConfigFetchFailed?.Invoke(ex.Message);
        }
    }

    /// <summary>
    /// Wait until config is ready, or timeout — whichever comes first.
    /// </summary>
    public async UniTask WaitUntilReadyAsync(int timeoutMs, CancellationToken ct)
    {
        if (IsInitialized)
            return;

        int elapsed = 0;
        const int stepMs = 100;

        while (!IsInitialized && IsFetching && elapsed < timeoutMs)
        {
            await UniTask.Delay(stepMs, cancellationToken: ct);
            elapsed += stepMs;
        }

        if (!IsInitialized)
            Debug.LogWarning("[RemoteConfig] Timed out waiting for config. Defaults will be used.");
    }

    // --- Callbacks ---

    private void OnFetchCompleted(ConfigResponse response)
    {
        RemoteConfigService.Instance.FetchCompleted -= OnFetchCompleted;
        IsFetching = false;

        if (response.requestOrigin == ConfigOrigin.Default)
            Debug.LogWarning("[RemoteConfig] No config on server. Using SDK defaults.");

        PopulateCache();

        IsInitialized = true;
        Debug.Log($"[RemoteConfig] Ready. {_cache.Count} key(s). Origin: {response.requestOrigin}");
        DebugDumpAllKeys();
        DebugAuditExpectedKeys();
        OnConfigFetched?.Invoke();
    }

    private void PopulateCache()
    {
        _cache.Clear();
        RuntimeConfig config = RemoteConfigService.Instance.appConfig;

        foreach (string key in config.GetKeys())
            _cache[key] = GetNativeValue(config, key);
    }

    private static object GetNativeValue(RuntimeConfig config, string key)
    {
        if (IsJsonKey(key))
        {
            string json = config.GetJson(key, "");
            if (!string.IsNullOrWhiteSpace(json) && json != "{}")
                return json;
            Debug.LogWarning($"[RemoteConfig] JSON key '{key}' returned empty from GetJson().");
            return "";
        }
        
        bool boolVal = config.GetBool(key);
        int intVal = config.GetInt(key);
        float floatVal = config.GetFloat(key);
        string stringVal = config.GetString(key);
        string raw = config.GetString(key);

        if (raw == "true" || raw == "false")
            return boolVal;

        if (Math.Abs(floatVal - intVal) > float.Epsilon)
            return floatVal;

        if (int.TryParse(raw, out _))
            return intVal;

        return stringVal;
    }
    
    public float GetFloat(string key, float defaultValue)
        => GetValue(key, defaultValue);
    
    public string GetJsonString(string key)
    {
        if (!IsInitialized)
            return null;
        
        string json = RemoteConfigService.Instance.appConfig.GetJson(key, "");
        if (!string.IsNullOrWhiteSpace(json) && json != "{}")
            return json;
       
        Debug.LogWarning($"[RemoteConfig] GetJsonString('{key}') empty — check dashboard publish + JSON content.");
        return null;
    }
    
    public void DebugDumpAllKeys()
    {
        if (!IsInitialized)
        {
            Debug.LogWarning("[RemoteConfig][DEBUG] Not initialized — cache empty.");
            return;
        }
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"[RemoteConfig][DEBUG] ===== {_cache.Count} key(s) in cache =====");
        foreach (var kvp in _cache)
        {
            string typeName = kvp.Value?.GetType().Name ?? "null";
            string preview = kvp.Value?.ToString() ?? "null";
            // Truncate long JSON so console stays readable
            if (preview.Length > 200)
                preview = preview.Substring(0, 200) + "... (truncated)";
            sb.AppendLine($"  [{typeName}] {kvp.Key} = {preview}");
        }
        Debug.Log(sb.ToString());
    }
    
    public void DebugAuditExpectedKeys()
    {
        string[] expectedKeys =
        {
            RemoteConfigKeys.MaxStamina,
            RemoteConfigKeys.MaxInteractions,
            RemoteConfigKeys.DailySubjectCount,
            RemoteConfigKeys.ExamDays5,
            RemoteConfigKeys.ExamDays30,
            RemoteConfigKeys.ExamDays120,
            RemoteConfigKeys.ExamDays360,
            RemoteConfigKeys.ExamScoreMultiplier,
            RemoteConfigKeys.QuestsConfig,
            RemoteConfigKeys.SubjectMath,
            RemoteConfigKeys.SubjectScience,
            RemoteConfigKeys.SubjectHistory,
            RemoteConfigKeys.SubjectGeography,
            RemoteConfigKeys.SubjectArts,
            RemoteConfigKeys.SubjectComputer,
            RemoteConfigKeys.SubjectRest,
            RemoteConfigKeys.ExamMath,
            RemoteConfigKeys.ExamScience,
            RemoteConfigKeys.ExamHistory,
            RemoteConfigKeys.ExamGeography,
            RemoteConfigKeys.ExamArts,
            RemoteConfigKeys.ExamComputer,
        };
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("[RemoteConfig][DEBUG] ===== Expected key audit =====");
        foreach (string key in expectedKeys)
        {
            bool present = HasKey(key);
            sb.AppendLine($"  {(present ? "OK  " : "MISS")}  {key}");
        }
        Debug.Log(sb.ToString());
    }
}