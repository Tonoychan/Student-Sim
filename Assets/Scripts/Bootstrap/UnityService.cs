using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.UI;

public class UnityService : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string environment = "development";
    [SerializeField] private string environmentID = "362ddb4d-6ced-4fff-a75f-e699a477cf90";
    
    [Header("Login Buttons")]
    [SerializeField] private Button _guestLoginButton;

    private PlayerAuthenticationService _playerAuthenticationService;
    private UnityRemoteConfigService _unityRemoteConfigService;
    private GameAnalyticsService _gameAnalyticsService;

    private void Awake()
    {
        InitializeServices().Forget();
    }

    async UniTaskVoid InitializeServices()
    {
        try
        {
            Debug.Log("[UGS] Initializing Unity Services...");

            var options = new InitializationOptions();
            options.SetEnvironmentName(environment);
           
            
            await UnityServices.InitializeAsync(options);
            
            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                Debug.Log("[UGS] ✅ Unity Services initialized successfully.");
                Debug.Log($"[UGS] Environment: {environment}");
                OnServicesReady();
            }
            else
            {
                Debug.LogWarning($"[UGS] ⚠️ Unexpected state after init: {UnityServices.State}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[UGS] ❌ Initialization failed: {e.Message}");
            Debug.LogException(e);
            OnServicesFailed(e);
        }
    }
    
    private void OnServicesReady()
    {
        // Hook in your services here, e.g.:
        // AuthenticationService.Instance.SignInAnonymouslyAsync();
        Debug.Log("[UGS] Services are ready to use.");
        _playerAuthenticationService = new PlayerAuthenticationService();
        _guestLoginButton.onClick.AddListener(_playerAuthenticationService.GuestLogin);
        
        _unityRemoteConfigService = new UnityRemoteConfigService();
        _unityRemoteConfigService.InitializeAndFetchAsync(environmentID).Forget();
        
        _gameAnalyticsService = new GameAnalyticsService();
        _gameAnalyticsService.InitAnalytics();
        _gameAnalyticsService.SendEvent("Game_Started");
        _gameAnalyticsService.SendEvent("newPlayer");
        _gameAnalyticsService.SendEvent("Game_Authenticated",new Dictionary<string, object>
        {
            {"player_ID",_playerAuthenticationService.GetPlayerID()},
            {"Environment",environment}
        });
        _gameAnalyticsService.Flush();
    }
    
    private void OnServicesFailed(Exception e)
    {
        // Handle failure gracefully, e.g. show offline mode UI
        Debug.LogError($"[UGS] Falling back to offline mode. Reason: {e.Message}");
    }
}
