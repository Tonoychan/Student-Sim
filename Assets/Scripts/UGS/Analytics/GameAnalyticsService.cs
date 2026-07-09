using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Services.Analytics;
using Unity.Services.Authentication;
using Unity.Services.Core;

/// <summary>
/// Sends custom analytics events to Unity Gaming Services.
/// </summary>
public class GameAnalyticsService
{
    public event Action OnInitialized;
    public event Action<string> OnInitializationFailed;
    
    public bool IsInitialized { get; private set; }
    
    /// <summary>Starts data collection. Safe to call once per session.</summary>
    public void InitAnalytics()
    {
        if (IsInitialized)
            return;
        
        AnalyticsService.Instance.StartDataCollection();
        IsInitialized = true;
        OnInitialized?.Invoke();
    }
    
    public void SendEvent(string eventName)
    {
        SendEvent(eventName, null);
    }

    /// <summary>Records a named event with optional parameters.</summary>
    public void SendEvent(string eventName, Dictionary<string, object> parameters)
    {
        if (string.IsNullOrWhiteSpace(eventName))
            return;

        try
        {
            CustomEvent analyticsEvent = new CustomEvent(eventName);

            if (parameters != null && parameters.Count > 0)
            {
                foreach (var kvp in parameters)
                {
                    if (kvp.Value == null)
                        continue;

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
                            break;
                    }
                }
            }

            AnalyticsService.Instance.RecordEvent(analyticsEvent);
        }
        catch (Exception)
        {
        }
    }
    
    /// <summary>Sends all queued events to the server immediately.</summary>
    public void Flush()
    {
        AnalyticsService.Instance.Flush();
    }
}
