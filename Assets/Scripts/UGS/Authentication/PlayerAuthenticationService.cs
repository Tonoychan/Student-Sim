using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;

/// <summary>
/// Signs the player in anonymously (guest login) and restores saved sessions.
/// </summary>
public class PlayerAuthenticationService
{
    /// <summary>True if a session token is stored on this device.</summary>
    public bool HasStoredSession =>
        AuthenticationService.Instance.SessionTokenExists;

    /// <summary>True if the player is currently signed in.</summary>
    public bool IsSignedIn =>
        AuthenticationService.Instance.IsSignedIn;
    
    /// <summary>Starts guest login (fire-and-forget).</summary>
    public void GuestLogin()
    {
        SignUpAnonymouslyAsync().Forget();
    }
    
    /// <summary>Tries to sign in using the stored session token.</summary>
    public async UniTask<bool> TryRestoreSessionAsync()
    {
        if (!AuthenticationService.Instance.SessionTokenExists)
            return false;
        return await SignInAnonymouslyAsync();
    }
    
    /// <summary>Creates or resumes an anonymous account.</summary>
    public async UniTask<bool> SignInAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            return AuthenticationService.Instance.IsSignedIn;
        }
        catch (AuthenticationException)
        {
            return false;
        }
        catch (RequestFailedException)
        {
            return false;
        }
    }
    
    async UniTask SignUpAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (AuthenticationException)
        {
        }
        catch (RequestFailedException)
        {
        }
    }
    
    /// <summary>Returns the current player ID, or empty if not signed in.</summary>
    public string GetPlayerID()
    {
        return AuthenticationService.Instance.IsSignedIn
            ? AuthenticationService.Instance.PlayerId
            : string.Empty;
    }
}
