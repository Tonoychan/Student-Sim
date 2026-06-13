using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.RemoteConfig;

/// <summary>
/// Manages Unity Remote Config initialization, fetching, and caching.
/// Access config values from anywhere via RemoteConfigManager.Instance.GetValue<T>(key).
/// </summary>
public class RemoteConfigManager : MonoBehaviour
{
    private const string EnvironmentId = "362ddb4d-6ced-4fff-a75f-e699a477cf90";
    // -------------------------------------------------------------------------
    // Singleton
    // -------------------------------------------------------------------------
    public static RemoteConfigManager Instance { get; private set; }

    // -------------------------------------------------------------------------
    // Events
    // -------------------------------------------------------------------------
    /// <summary>Fired when the config cache has been successfully populated.</summary>
    public event Action OnConfigFetched;

    /// <summary>Fired when fetching fails. Arg is the exception message.</summary>
    public event Action<string> OnConfigFetchFailed;

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------
    private Dictionary<string, object> _cache = new();

    public bool IsInitialized { get; private set; }
    public bool IsFetching    { get; private set; }

    // -------------------------------------------------------------------------
    // Structs required by the Remote Config SDK
    // -------------------------------------------------------------------------
    private struct UserAttributes  { }
    private struct AppAttributes   { }

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        await InitializeAndFetchAsync();
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initializes Unity Services, signs in anonymously if needed, then fetches
    /// the Remote Config and stores every key/value pair in the cache.
    /// Safe to call multiple times — returns immediately if already initialized.
    /// </summary>
    public async Task InitializeAndFetchAsync()
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
            // 1. Boot Unity Gaming Services
            await UnityServices.InitializeAsync();
            Debug.Log("[RemoteConfig] Unity Services initialized.");

            // 2. Authenticate (anonymous sign-in is required by Remote Config)
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"[RemoteConfig] Signed in anonymously. Player ID: {AuthenticationService.Instance.PlayerId}");
            }

            // 3. Register the fetch-completed callback and request config
            RemoteConfigService.Instance.FetchCompleted += OnFetchCompleted;
            RemoteConfigService.Instance.SetEnvironmentID(EnvironmentId);
            await RemoteConfigService.Instance.FetchConfigsAsync(new UserAttributes(), new AppAttributes());
        }
        catch (Exception ex)
        {
            IsFetching = false;
            Debug.LogError($"[RemoteConfig] Initialization/fetch failed: {ex.Message}");
            OnConfigFetchFailed?.Invoke(ex.Message);
        }
    }

    /// <summary>
    /// Re-fetches the Remote Config from the server and refreshes the cache.
    /// Useful for refreshing values at runtime (e.g. on scene load or app resume).
    /// </summary>
    public async Task RefreshAsync()
    {
        if (IsFetching)
        {
            Debug.LogWarning("[RemoteConfig] A fetch is already in progress.");
            return;
        }

        IsFetching     = true;
        IsInitialized  = false;

        try
        {
            RemoteConfigService.Instance.FetchCompleted -= OnFetchCompleted; // avoid duplicate subscriptions
            RemoteConfigService.Instance.FetchCompleted += OnFetchCompleted;
            await RemoteConfigService.Instance.FetchConfigsAsync(new UserAttributes(), new AppAttributes());
        }
        catch (Exception ex)
        {
            IsFetching = false;
            Debug.LogError($"[RemoteConfig] Refresh failed: {ex.Message}");
            OnConfigFetchFailed?.Invoke(ex.Message);
        }
    }

    /// <summary>
    /// Returns a cached config value cast to the requested type.
    /// Falls back to <paramref name="defaultValue"/> when the key is missing or cast fails.
    /// </summary>
    /// <typeparam name="T">Expected type: bool, int, float, string, or JSON-deserializable type.</typeparam>
    public T GetValue<T>(string key, T defaultValue = default)
    {
        if (!IsInitialized)
        {
            Debug.LogWarning($"[RemoteConfig] Config not yet fetched. Returning default for '{key}'.");
            return defaultValue;
        }

        if (!_cache.TryGetValue(key, out object raw))
        {
            Debug.LogWarning($"[RemoteConfig] Key '{key}' not found in cache. Returning default.");
            return defaultValue;
        }

        try
        {
            // Direct cast covers bool, int, float, string, long, etc.
            if (raw is T directCast)
                return directCast;

            // Convert handles numeric widening (e.g. long → int, double → float)
            return (T)Convert.ChangeType(raw, typeof(T));
        }
        catch
        {
            Debug.LogWarning($"[RemoteConfig] Could not cast '{key}' (value: {raw}) to {typeof(T).Name}. Returning default.");
            return defaultValue;
        }
    }

    /// <summary>Returns true when the key exists in the cache.</summary>
    public bool HasKey(string key) => _cache.ContainsKey(key);

    /// <summary>Read-only snapshot of all cached key/value pairs.</summary>
    public IReadOnlyDictionary<string, object> GetAllValues() => _cache;

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------
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

    /// <summary>
    /// Probes the SDK for the native type of each key and stores it accordingly.
    /// Order matters: bool must come before int (bools are ints at JSON level).
    /// </summary>
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
    
    public void DebugPrintAllConfigs()
    {
        var values = RemoteConfigManager.Instance.GetAllValues();

        if (values.Count == 0)
        {
            Debug.LogWarning("[RemoteConfig] Cache is empty or not yet fetched.");
            return;
        }

        System.Text.StringBuilder sb = new();
        sb.AppendLine($"[RemoteConfig] ===== {values.Count} key(s) =====");

        foreach (var kvp in values)
            sb.AppendLine($"  {kvp.Key} ({kvp.Value?.GetType().Name}) = {kvp.Value}");

        Debug.Log(sb.ToString());
    }
}