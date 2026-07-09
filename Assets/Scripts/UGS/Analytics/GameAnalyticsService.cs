using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Services.Analytics;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class GameAnalyticsService
{
    public event Action OnInitialized;
    public event Action<string> OnInitializationFailed;
    
    public bool IsInitialized { get; private set; }
    
    public void InitAnalytics()
    {
        if (IsInitialized)
        {
            Debug.Log("[Analytics] Already initialized. Skipping.");
            return;
        }
        
        AnalyticsService.Instance.StartDataCollection(); // <-- this is critical
        Debug.Log("[Analytics] Data collection started.");
        IsInitialized = true;
        Debug.Log("[Analytics] Ready.");
        OnInitialized?.Invoke();
    }
    
    public void SendEvent(string eventName)
    {
        SendEvent(eventName, null);
    }

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
                        Debug.LogWarning(
                            $"[Analytics] Skipping null value for parameter '{kvp.Key}' in event '{eventName}'.");
                        continue;
                    }

                    switch (kvp.Value)
                    {
                        case string v: analyticsEvent.Add(kvp.Key, v); break;
                        case bool v: analyticsEvent.Add(kvp.Key, v); break;
                        case int v: analyticsEvent.Add(kvp.Key, v); break;
                        case long v: analyticsEvent.Add(kvp.Key, v); break;
                        case float v: analyticsEvent.Add(kvp.Key, v); break;
                        case double v: analyticsEvent.Add(kvp.Key, v); break;
                        default:
                            analyticsEvent.Add(kvp.Key, kvp.Value.ToString());
                            Debug.LogWarning(
                                $"[Analytics] Parameter '{kvp.Key}' has unsupported type '{kvp.Value.GetType().Name}'. Serialized to string.");
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
    
    public void Flush()
    {
        AnalyticsService.Instance.Flush();
        Debug.Log("[Analytics] Flushed all queued events.");
    }
}
