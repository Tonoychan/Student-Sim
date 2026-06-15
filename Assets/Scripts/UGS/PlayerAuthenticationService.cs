using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class PlayerAuthenticationService
{
    public bool HasStoredSession =>
        AuthenticationService.Instance.SessionTokenExists;
    public bool IsSignedIn =>
        AuthenticationService.Instance.IsSignedIn;
    
    public void GuestLogin()
    {
        SignUpAnonymouslyAsync().Forget();
    }
    
    public async UniTask<bool> TryRestoreSessionAsync()
    {
        if (!AuthenticationService.Instance.SessionTokenExists)
            return false;
        return await SignInAnonymouslyAsync();
    }
    
    public async UniTask<bool> SignInAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log(AuthenticationService.Instance.SessionTokenExists
                ? "Resumed existing session."
                : "Created new anonymous account.");
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
            return AuthenticationService.Instance.IsSignedIn;
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
            return false;
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
            return false;
        }
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
    
    public string GetPlayerID()
    {
        return AuthenticationService.Instance.IsSignedIn
            ? AuthenticationService.Instance.PlayerId
            : string.Empty;
    }
}
