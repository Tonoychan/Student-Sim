using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Runs on the login scene. Starts Unity Gaming Services, signs the player in,
/// then loads the selection scene.
/// </summary>
public class UnityService : MonoBehaviour
{
    private const string SelectionSceneName = "SelectionScene";
    private string _environmentId;
    [Header("Settings")]
    [SerializeField] private string environment = "development";
    [SerializeField] private string environmentID = "362ddb4d-6ced-4fff-a75f-e699a477cf90";
    
    [Header("Login Buttons")]
    [SerializeField] private Button _guestLoginButton;

    private PlayerAuthenticationService _playerAuthenticationService;
    private UnityRemoteConfigService _unityRemoteConfigService;
    private GameAnalyticsService _gameAnalyticsService;
    private TermLeaderboardService _leaderboardService;
    private PlayerCloudSaveService _playerCloudSaveService;

    private void Start()
    {
        _environmentId = environmentID;
        InitializeServices().Forget();
    }

    /// <summary>Connects to Unity Services (auth, remote config, analytics).</summary>
    async UniTaskVoid InitializeServices()
    {
        try
        {
            var options = new InitializationOptions();
            options.SetEnvironmentName(environment);
            
            await UnityServices.InitializeAsync(options);
            
            if (UnityServices.State == ServicesInitializationState.Initialized)
                OnServicesReady();
        }
        catch (Exception e)
        {
            Debug.LogError($"[UGS] Initialization failed: {e.Message}");
            OnServicesFailed(e);
        }
    }
    
    /// <summary>Starts child services after UGS init succeeds.</summary>
    private void OnServicesReady()
    {
        _playerAuthenticationService = new PlayerAuthenticationService();
        
        _unityRemoteConfigService = new UnityRemoteConfigService();
        _unityRemoteConfigService.InitializeAndFetchAsync(_environmentId).Forget();
        
        _gameAnalyticsService = new GameAnalyticsService();
        _gameAnalyticsService.InitAnalytics();
        _gameAnalyticsService.SendEvent("Game_Started");
        _gameAnalyticsService.Flush();
        
        TryAutoLoginOrShowGuestButton().Forget();
    }
    
    /// <summary>Lets the player continue offline if UGS fails to start.</summary>
    private void OnServicesFailed(Exception e)
    {
        Debug.LogError($"[UGS] Falling back to offline mode. Reason: {e.Message}");
        if (_guestLoginButton != null)
            _guestLoginButton.onClick.AddListener(OnGuestLoginOfflineFallback);
    }
    
    private void OnGuestLoginOfflineFallback()
    {
        LoadSelectionScene();
    }
    
    /// <summary>Auto-login if a saved session exists, otherwise show the guest button.</summary>
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
    
    /// <summary>Runs leaderboard setup and cloud save sync after login.</summary>
    private async UniTask OnAuthenticatedAsync()
    {
        SendAuthenticatedAnalytics();
        _leaderboardService = new TermLeaderboardService();
        await _leaderboardService.EnsurePlayerNameIsIdAsync();
        await _leaderboardService.EnsureServiceLoadedAsync();
        _playerCloudSaveService = new PlayerCloudSaveService();
        try
        {
            await _playerCloudSaveService.SyncCloudToLocalAsync();
        }
        catch (Exception)
        {
            // Cloud sync must not block login.
        }
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
