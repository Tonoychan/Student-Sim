using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Exceptions;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

public class TermLeaderboardService
{
    static readonly Dictionary<string, LeaderboardScoresPage> ScoreCache = new();
    
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
    
    public UniTask<bool> EnsureServiceLoadedAsync()
    {
        ILeaderboardsService service = GetLeaderboards();
        if (service == null)
        {
            Debug.LogError("[Leaderboard] Leaderboards not available from UGS registry.");
            return UniTask.FromResult(false);
        }
        Debug.Log("[Leaderboard] Service ready.");
        return UniTask.FromResult(true);
    }
    
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
    
    public async UniTask<bool> SubmitScoreAsync(int maxDays, int finalScore)
    {
        string leaderboardId = TermLeaderboardIds.GetForMaxDays(maxDays);
        if (string.IsNullOrEmpty(leaderboardId))
        {
            Debug.LogWarning($"[Leaderboard] No board for {maxDays}-day mode.");
            return false;
        }
        if (!CanUseLeaderboards)
        {
            Debug.LogError("[Leaderboard] Not ready or player not signed in.");
            return false;
        }
        try
        {
            if (!await EnsureServiceLoadedAsync())
                return false;
            await EnsurePlayerNameIsIdAsync();
            ILeaderboardsService leaderboards = GetLeaderboards();
            if (leaderboards == null)
                return false;
            await leaderboards.AddPlayerScoreAsync(leaderboardId, finalScore);
            Debug.Log($"[Leaderboard] Submitted {finalScore} to {leaderboardId}");
            
            InvalidateCache(leaderboardId);  
            return true;
        }
        catch (LeaderboardsException ex)
        {
            if (ex.Message.Contains("could not be found"))
                Debug.LogWarning($"[Leaderboard] Board '{leaderboardId}' not found in dashboard (development env).");
            else
                Debug.LogException(ex);
            return false;
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
            return false;
        }
        
    }
    
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
        catch (LeaderboardsException ex)
        {
            Debug.LogException(ex);
            return null;
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
            return null;
        }
    }
}
