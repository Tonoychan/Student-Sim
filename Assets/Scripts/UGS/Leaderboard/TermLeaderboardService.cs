using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Exceptions;
using Unity.Services.Leaderboards.Models;

/// <summary>
/// Submits term scores and fetches leaderboard rankings from Unity Gaming Services.
/// </summary>
public class TermLeaderboardService
{
    static readonly Dictionary<string, LeaderboardScoresPage> ScoreCache = new();
    
    /// <summary>Clears cached scores so the next fetch gets fresh data.</summary>
    public static void InvalidateCache(string leaderboardId = null)
    {
        if (string.IsNullOrEmpty(leaderboardId))
            ScoreCache.Clear();
        else
            ScoreCache.Remove(leaderboardId);
    }
    
    public bool IsAvailable =>
        UnityServices.State == ServicesInitializationState.Initialized;
    
    public bool CanUseLeaderboards =>
        IsAvailable && AuthenticationService.Instance.IsSignedIn;
    
    ILeaderboardsService GetLeaderboards()
    {
        if (!CanUseLeaderboards)
            return null;
        ILeaderboardsService service = UnityServices.Instance.GetLeaderboardsService();
        if (service != null)
            return service;
        try
        {
            return LeaderboardsService.Instance;
        }
        catch (ServicesInitializationException)
        {
            return null;
        }
    }
    
    /// <summary>Checks that the leaderboard service is ready to use.</summary>
    public UniTask<bool> EnsureServiceLoadedAsync()
    {
        return UniTask.FromResult(GetLeaderboards() != null);
    }
    
    /// <summary>Sets the player's display name to their ID (for leaderboard entries).</summary>
    public async UniTask<bool> EnsurePlayerNameIsIdAsync()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
            return false;
        try
        {
            string playerId = AuthenticationService.Instance.PlayerId;
            await AuthenticationService.Instance.UpdatePlayerNameAsync(playerId);
            return true;
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
    
    /// <summary>Posts the final term score to the leaderboard for that term length.</summary>
    public async UniTask<bool> SubmitScoreAsync(int maxDays, int finalScore)
    {
        string leaderboardId = TermLeaderboardIds.GetForMaxDays(maxDays);
        if (string.IsNullOrEmpty(leaderboardId) || !CanUseLeaderboards)
            return false;
        try
        {
            if (!await EnsureServiceLoadedAsync())
                return false;
            await EnsurePlayerNameIsIdAsync();
            ILeaderboardsService leaderboards = GetLeaderboards();
            if (leaderboards == null)
                return false;
            await leaderboards.AddPlayerScoreAsync(leaderboardId, finalScore);
            
            InvalidateCache(leaderboardId);  
            return true;
        }
        catch (LeaderboardsException)
        {
            return false;
        }
        catch (RequestFailedException)
        {
            return false;
        }
        
    }
    
    /// <summary>Fetches top scores for a leaderboard. Uses cache unless forceRefresh is true.</summary>
    public async UniTask<LeaderboardScoresPage> GetScoresAsync(
        string leaderboardId,
        int limit = 20,
        bool forceRefresh = false)
    {
        if (!CanUseLeaderboards)
            return null;
        
        if (!forceRefresh &&
            ScoreCache.TryGetValue(leaderboardId, out LeaderboardScoresPage cached))
        {
            return cached;
        }
        
        try
        {
            if (!await EnsureServiceLoadedAsync())
                return null;
            ILeaderboardsService leaderboards = GetLeaderboards();
            if (leaderboards == null)
                return null;
            LeaderboardScoresPage page = await leaderboards.GetScoresAsync(
                leaderboardId,
                new GetScoresOptions { Limit = limit, Offset = 0 });
            if (page != null)
                ScoreCache[leaderboardId] = page;
            return page;
        }
        catch (LeaderboardsException)
        {
            return null;
        }
        catch (RequestFailedException)
        {
            return null;
        }
    }
}
