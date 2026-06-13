using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Services.RemoteConfig;
using UnityEngine;

public class UnityRemoteConfigService 
{
    public static UnityRemoteConfigService Instance { get; private set; }
    
    public event Action OnConfigFetched;
    public event Action<string> OnConfigFetchFailed;

    public Dictionary<string, object> _cache = new();
    
    public bool IsInitialized { get; private set; }
    public bool IsFetching    { get; private set; }
    
    private struct UserAttributes  { }
    private struct AppAttributes   { }

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
            await RemoteConfigService.Instance.FetchConfigsAsync(new UserAttributes(), new AppAttributes());
        }
        catch (Exception ex)
        {
            IsFetching = false;
            Debug.LogError($"[RemoteConfig] Initialization/fetch failed: {ex.Message}");
            OnConfigFetchFailed?.Invoke(ex.Message);
        }
    }
    
    private void OnFetchCompleted(ConfigResponse response)
    {
        RemoteConfigService.Instance.FetchCompleted -= OnFetchCompleted;
        IsFetching = false;

        if (response.requestOrigin == ConfigOrigin.Default)
        {
            Debug.LogWarning("[RemoteConfig] No config found on server. Using SDK defaults.");
        }

        PopulateCache();

        IsInitialized = true;
        Debug.Log($"[RemoteConfig] Cache populated with {_cache.Count} key(s). Origin: {response.requestOrigin}");
        OnConfigFetched?.Invoke();
    }
    
    private void PopulateCache()
    {
        _cache.Clear();
        RuntimeConfig config = RemoteConfigService.Instance.appConfig;

        foreach (string key in config.GetKeys())
        {
            // Store every value as its native type using the SDK helper
            _cache[key] = GetNativeValue(config, key);
        }
    }
    
    private static object GetNativeValue(RuntimeConfig config, string key)
    {
        // Try bool first (JSON true/false)
        bool   boolVal   = config.GetBool(key);
        int    intVal    = config.GetInt(key);
        float  floatVal  = config.GetFloat(key);
        string stringVal = config.GetString(key);

        // Heuristic: if the raw JSON string is "true" or "false" it's a bool
        string raw = config.GetString(key);
        if (raw == "true" || raw == "false") return boolVal;

        // If int and float differ significantly, it's a float
        if (Math.Abs(floatVal - intVal) > float.Epsilon) return floatVal;

        // If it parses as an integer without loss, store int
        if (int.TryParse(raw, out _)) return intVal;

        // Otherwise store as string (covers JSON objects/arrays too)
        return stringVal;
    }
}
