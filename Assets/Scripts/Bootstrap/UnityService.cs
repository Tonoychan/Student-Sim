using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Services.Leaderboards;

public class UnityService : MonoBehaviour
{
    private const string SelectionSceneName = "SelectionScene";
    [Header("Settings")]
    [SerializeField] private string environment = "development";
    [SerializeField] private string environmentID = "362ddb4d-6ced-4fff-a75f-e699a477cf90";
    
    [Header("Login Buttons")]
    [SerializeField] private Button _guestLoginButton;

    private PlayerAuthenticationService _playerAuthenticationService;
    private UnityRemoteConfigService _unityRemoteConfigService;
    private GameAnalyticsService _gameAnalyticsService;
    private TermLeaderboardService _leaderboardService;

    private void Start()
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
        
        _unityRemoteConfigService = new UnityRemoteConfigService();
        _unityRemoteConfigService.InitializeAndFetchAsync(environmentID).Forget();
        
        _gameAnalyticsService = new GameAnalyticsService();
        _gameAnalyticsService.InitAnalytics();
        _gameAnalyticsService.SendEvent("Game_Started");
        _gameAnalyticsService.Flush();
        
        TryAutoLoginOrShowGuestButton().Forget();
    }
    
    private void OnServicesFailed(Exception e)
    {
        // Handle failure gracefully, e.g. show offline mode UI
        Debug.LogError($"[UGS] Falling back to offline mode. Reason: {e.Message}");
        if (_guestLoginButton != null)
            _guestLoginButton.onClick.AddListener(OnGuestLoginOfflineFallback);
    }
    
    private void OnGuestLoginOfflineFallback()
    {
        Debug.LogWarning("[UGS] Offline fallback → SelectionScene");
        LoadSelectionScene();
    }
    
    private async UniTaskVoid TryAutoLoginOrShowGuestButton()
    {
        if (_playerAuthenticationService.HasStoredSession)
        {
            bool restored = await _playerAuthenticationService.TryRestoreSessionAsync();
            if (restored)
            {
                await OnAuthenticatedAsync();
                LoadSelectionScene();
                return;
            }
        }
        if (_guestLoginButton != null)
            _guestLoginButton.onClick.AddListener(OnGuestLoginClicked);
    }
    
    private void OnGuestLoginClicked()
    {
        GuestLoginAndProceed().Forget();
    }
    private async UniTaskVoid GuestLoginAndProceed()
    {
        _guestLoginButton.interactable = false;
        bool success = await _playerAuthenticationService.SignInAnonymouslyAsync();
        if (success)
        {
            await OnAuthenticatedAsync();
            LoadSelectionScene();
        }
        else
        {
            _guestLoginButton.interactable = true;
        }
    }
    
    private async UniTask OnAuthenticatedAsync()
    {
        SendAuthenticatedAnalytics();
        _leaderboardService = new TermLeaderboardService();
        await _leaderboardService.EnsurePlayerNameIsIdAsync();
        bool lbReady = await _leaderboardService.EnsureServiceLoadedAsync();
        if (!lbReady)
            Debug.LogWarning("[Leaderboard] Not ready at login; will retry when opening panel or submitting score.");
    }
    
    private void SendAuthenticatedAnalytics()
    {
        _gameAnalyticsService.SendEvent("Game_Authenticated", new Dictionary<string, object>
        {
            { "player_ID", _playerAuthenticationService.GetPlayerID() },
            { "Environment", environment }
        });
        _gameAnalyticsService.Flush();
    }
    private static void LoadSelectionScene()
    {
        SceneManager.LoadScene(SelectionSceneName);
    }
}
