using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class PlayerAuthenticationService
{
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
    
    public string GetPlayerID()
    {
        return AuthenticationService.Instance.PlayerId;
    }
}
