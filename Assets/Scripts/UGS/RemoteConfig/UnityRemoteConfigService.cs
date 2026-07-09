using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Services.RemoteConfig;
using UnityEngine;

/// <summary>
/// Fetches game tuning values from Unity Remote Config and caches them in memory.
/// Falls back to defaults if a key is missing.
/// </summary>
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

    /// <summary>Reads a cached value, or returns the default if missing.</summary>
    public T GetValue<T>(string key, T defaultValue = default)
    {
        if (!IsInitialized)
            return defaultValue;

        if (!_cache.TryGetValue(key, out object raw) || raw == null)
            return defaultValue;

        try
        {
            if (raw is T direct)
                return direct;

            return (T)Convert.ChangeType(raw, typeof(T));
        }
        catch (Exception)
        {
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

    public async UniTask InitRemoteConfig(string environmentId)
    {
        await InitializeAndFetchAsync(environmentId);
    }

    /// <summary>Downloads config from the server for the first time.</summary>
    public async UniTask InitializeAndFetchAsync(string environmentId)
    {
        if (IsInitialized || IsFetching)
            return;

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

    /// <summary>Re-downloads config (clears cache first).</summary>
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

    /// <summary>Waits until config is ready or the timeout is reached.</summary>
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
    }

    private void OnFetchCompleted(ConfigResponse response)
    {
        RemoteConfigService.Instance.FetchCompleted -= OnFetchCompleted;
        IsFetching = false;

        PopulateCache();

        IsInitialized = true;
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
    
    /// <summary>Returns raw JSON for a config key (subjects, exams, quests, etc.).</summary>
    public string GetJsonString(string key)
    {
        if (!IsInitialized)
            return null;
        
        string json = RemoteConfigService.Instance.appConfig.GetJson(key, "");
        if (!string.IsNullOrWhiteSpace(json) && json != "{}")
            return json;

        return null;
    }
}
