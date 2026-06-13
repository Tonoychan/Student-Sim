using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

public class InitializeUnityServices : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string environment = "development"; // or "development"

    // private void OnEnable()
    // {
    //     // Hook the callback ONCE when the object is active
    //     PlayerAccountService.Instance.SignedIn += OnPlayerAccountSignedIn;
    // }
    //
    // private void OnDisable()
    // {
    //     // Always unsubscribe to avoid duplicate calls or memory leaks
    //     PlayerAccountService.Instance.SignedIn -= OnPlayerAccountSignedIn;
    // }
    private async void Start()
    {
        await InitializeUnityService();
    }

    public async UniTask InitializeUnityService()
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
    }

    private void OnServicesFailed(Exception e)
    {
        // Handle failure gracefully, e.g. show offline mode UI
        Debug.LogError($"[UGS] Falling back to offline mode. Reason: {e.Message}");
    }

    public void GuestLogin()
    {
        SignUpAnonymouslyAsync().Forget();
    }

    async UniTask SignUpAnonymouslyAsync()
    {
        try
        {
            if (AuthenticationService.Instance.SessionTokenExists)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Resumed existing session.");
            }
            else
            {
                // First time — create a new anonymous account
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Created new anonymous account.");
            }

            // Shows how to get the playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

    public void ClearCacheAndSignOut()
    {
        try
        {
            // Always safe to call
            AuthenticationService.Instance.SignOut(clearCredentials: true);
            Debug.Log("Auth cache cleared.");
            Debug.Log("Signed out successfully. Ready for fresh login.");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

}
