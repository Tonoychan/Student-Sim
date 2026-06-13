using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Analytics;

/// <summary>
/// Manages Unity Analytics custom event tracking.
/// Assumes Unity Services and Authentication are already initialized elsewhere (e.g. RemoteConfigManager).
/// Send events from anywhere via AnalyticsManager.Instance.SendEvent(name, params).
/// </summary>
public class AnalyticsManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Singleton
    // -------------------------------------------------------------------------
    public static AnalyticsManager Instance { get; private set; }

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

    // -------------------------------------------------------------------------
    // Public API — Custom Events
    // -------------------------------------------------------------------------

    /// <summary>
    /// Sends a custom event with no parameters.
    /// </summary>
    public void SendEvent(string eventName)
    {
        SendEvent(eventName, null);
    }

    /// <summary>
    /// Sends a custom event with a flexible key-value parameter dictionary.
    /// Supported value types: string, bool, int, long, float, double.
    /// </summary>
    public void SendEvent(string eventName, Dictionary<string, object> parameters)
    {
        if (string.IsNullOrWhiteSpace(eventName))
        {
            Debug.LogError("[Analytics] Event name cannot be null or empty.");
            return;
        }

        try
        {
            CustomEvent analyticsEvent = new CustomEvent(eventName);

            if (parameters != null && parameters.Count > 0)
            {
                foreach (var kvp in parameters)
                {
                    if (kvp.Value == null)
                    {
                        Debug.LogWarning($"[Analytics] Skipping null value for parameter '{kvp.Key}' in event '{eventName}'.");
                        continue;
                    }

                    switch (kvp.Value)
                    {
                        case string v: analyticsEvent.Add(kvp.Key, v); break;
                        case bool   v: analyticsEvent.Add(kvp.Key, v); break;
                        case int    v: analyticsEvent.Add(kvp.Key, v); break;
                        case long   v: analyticsEvent.Add(kvp.Key, v); break;
                        case float  v: analyticsEvent.Add(kvp.Key, v); break;
                        case double v: analyticsEvent.Add(kvp.Key, v); break;
                        default:
                            analyticsEvent.Add(kvp.Key, kvp.Value.ToString());
                            Debug.LogWarning($"[Analytics] Parameter '{kvp.Key}' has unsupported type '{kvp.Value.GetType().Name}'. Serialized to string.");
                            break;
                    }
                }
            }

            AnalyticsService.Instance.RecordEvent(analyticsEvent);
            Debug.Log($"[Analytics] Event recorded: '{eventName}' with {parameters?.Count ?? 0} parameter(s).");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Analytics] Failed to record event '{eventName}': {ex.Message}");
        }
    }

    /// <summary>
    /// Fluent builder — construct and send an event in a chain.
    /// <example>
    /// AnalyticsManager.Instance
    ///     .Event("shop_opened")
    ///     .Param("source", "main_menu")
    ///     .Param("player_level", 5)
    ///     .Send();
    /// </example>
    /// </summary>
    public EventBuilder Event(string eventName) => new EventBuilder(this, eventName);

    // -------------------------------------------------------------------------
    // Flush
    // -------------------------------------------------------------------------

    /// <summary>
    /// Forces an immediate upload of all queued events.
    /// Call before scene transitions, app quit, or other critical moments.
    /// </summary>
    public void Flush()
    {
        AnalyticsService.Instance.Flush();
        Debug.Log("[Analytics] Flushed all queued events.");
    }

    private void OnApplicationQuit() => Flush();

    // -------------------------------------------------------------------------
    // Fluent EventBuilder
    // -------------------------------------------------------------------------
    public class EventBuilder
    {
        private readonly AnalyticsManager _manager;
        private readonly string _eventName;
        private readonly Dictionary<string, object> _params = new();

        internal EventBuilder(AnalyticsManager manager, string eventName)
        {
            _manager   = manager;
            _eventName = eventName;
        }

        public EventBuilder Param(string key, object value)
        {
            _params[key] = value;
            return this;
        }

        public void Send() => _manager.SendEvent(_eventName, _params);
    }
}