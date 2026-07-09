using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using UnityEngine;

public class PlayerCloudSaveService
{
    //Add — singleton (same pattern as UnityRemoteConfigService)
    public static PlayerCloudSaveService Instance { get; private set; }
    //Add
    private const int DebounceDelayMs = 35_000;
    //Add
    private bool _termDirty;
    private bool _accountDirty;
    private string _pendingTermJson;
    private string _pendingAccountJson;
    private CancellationTokenSource _debounceCts;
    //Add
    public PlayerCloudSaveService()
    {
        Instance = this;
    }
    public bool CanUseCloudSave =>
        AuthenticationService.Instance.IsSignedIn;

    /// <summary>
    /// Phase 2a: save + load one test key to verify dashboard + auth.
    /// </summary>
    public async UniTask<bool> RunConnectivityTestAsync()
    {
        if (!CanUseCloudSave)
        {
            Debug.LogWarning("[CloudSave] Player not signed in — skipping test.");
            return false;
        }

        string playerId = AuthenticationService.Instance.PlayerId;
        string testValue = $"StudentSim ping @ {DateTime.UtcNow:O}";

        try
        {
            //Add — SAVE test key
            var saveData = new Dictionary<string, object>
            {
                { CloudSaveKeys.TestPing, testValue }
            };

            await CloudSaveService.Instance.Data.Player.SaveAsync(saveData);
            Debug.Log($"[CloudSave] TEST SAVE OK. key={CloudSaveKeys.TestPing}, value={testValue}, playerId={playerId}");

            //Add — LOAD same key back
            var keys = new HashSet<string> { CloudSaveKeys.TestPing };
            var loaded = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);

            if (loaded.TryGetValue(CloudSaveKeys.TestPing, out Item item))
            {
                string readBack = item.Value.GetAsString();
                Debug.Log($"[CloudSave] TEST LOAD OK. readBack={readBack}");
                return readBack == testValue;
            }

            Debug.LogWarning("[CloudSave] TEST LOAD: key not found in response.");
            return false;
        }
        catch (CloudSaveException ex)
        {
            Debug.LogError($"[CloudSave] CloudSaveException: {ex.Message}");
            Debug.LogException(ex);
            return false;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CloudSave] Unexpected error: {ex.Message}");
            Debug.LogException(ex);
            return false;
        }
    }

    //Add — generic helpers for Phase 2b
    public async UniTask SaveKeysAsync(Dictionary<string, object> data)
    {
        if (!CanUseCloudSave || data == null || data.Count == 0)
            return;

        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
    }

    public async UniTask<Dictionary<string, Item>> LoadKeysAsync(ISet<string> keys)
    {
        if (!CanUseCloudSave || keys == null || keys.Count == 0)
            return new Dictionary<string, Item>();

        return await CloudSaveService.Instance.Data.Player.LoadAsync(keys);
    }
    
    //Add — queue term blob (debounced cloud upload)
    public void QueueTermSave(PlayerSaveData data)
    {
        if (!CanUseCloudSave || data == null)
            return;
        data.lastSavedUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        _pendingTermJson = JsonUtility.ToJson(data);
        _termDirty = true;
        ScheduleDebouncedFlush().Forget();
    }
    //Add
    public void QueueAccountSave(PlayerAccountData data)
    {
        if (!CanUseCloudSave || data == null)
            return;
        data.lastSavedUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        _pendingAccountJson = JsonUtility.ToJson(data);
        _accountDirty = true;
        ScheduleDebouncedFlush().Forget();
    }
    //Add
    async UniTaskVoid ScheduleDebouncedFlush()
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;
        try
        {
            await UniTask.Delay(DebounceDelayMs, cancellationToken: token);
            await FlushDirtyKeysAsync();
        }
        catch (OperationCanceledException)
        {
            // reset by newer save — expected
        }
    }
    //Add — upload only dirty keys
    public async UniTask ForceFlushAsync()
    {
        _debounceCts?.Cancel();
        await FlushDirtyKeysAsync();
    }
    //Add
    async UniTask FlushDirtyKeysAsync()
    {
        if (!CanUseCloudSave)
            return;
        var payload = new Dictionary<string, object>();
        if (_termDirty && !string.IsNullOrEmpty(_pendingTermJson))
            payload[CloudSaveKeys.TermSave] = _pendingTermJson;
        if (_accountDirty && !string.IsNullOrEmpty(_pendingAccountJson))
            payload[CloudSaveKeys.AccountSave] = _pendingAccountJson;
        if (payload.Count == 0)
            return;
        await SaveKeysAsync(payload);
        if (_termDirty) _termDirty = false;
        if (_accountDirty) _accountDirty = false;
        Debug.Log($"[CloudSave] Flushed {payload.Count} key(s) to cloud.");
    }
    //Add — read JSON string from cloud Item
    static string GetJsonFromItem(Item item)
    {
        if (item == null || item.Value == null)
            return null;
        return item.Value.GetAsString();
    }
    //Add — login merge: cloud ↔ PlayerPrefs
    public async UniTask SyncCloudToLocalAsync()
    {
        if (!CanUseCloudSave)
            return;
        var localTermProvider = new PlayerPrefsSaveProvider();
        var localAccountProvider = new PlayerAccountSaveProvider();
        var keys = new HashSet<string>
        {
            CloudSaveKeys.TermSave,
            CloudSaveKeys.AccountSave
        };
        var loaded = await LoadKeysAsync(keys);
        SyncTermKey(loaded, localTermProvider);
        SyncAccountKey(loaded, localAccountProvider);
        // push any pending dirty data immediately after migration
        await FlushDirtyKeysAsync();
    }
    //Add
    void SyncTermKey(Dictionary<string, Item> loaded, PlayerPrefsSaveProvider local)
    {
        bool hasCloud = loaded.TryGetValue(CloudSaveKeys.TermSave, out Item cloudItem);
        string cloudJson = hasCloud ? GetJsonFromItem(cloudItem) : null;
        PlayerSaveData cloudData = !string.IsNullOrEmpty(cloudJson)
            ? JsonUtility.FromJson<PlayerSaveData>(cloudJson)
            : null;
        bool hasLocal = local.HasSave();
        PlayerSaveData localData = hasLocal ? local.Load() : null;
        if (cloudData != null && hasLocal)
        {
            if (cloudData.lastSavedUtc >= localData.lastSavedUtc)
            {
                local.Save(cloudData);
                Debug.Log("[CloudSave] Term: cloud newer → copied to PlayerPrefs.");
            }
            else
            {
                QueueTermSave(localData);
                Debug.Log("[CloudSave] Term: local newer → queued upload.");
            }
        }
        else if (cloudData != null)
        {
            local.Save(cloudData);
            Debug.Log("[CloudSave] Term: local empty → restored from cloud.");
        }
        else if (hasLocal && localData != null && !localData.termCompleted)
        {
            QueueTermSave(localData);
            Debug.Log("[CloudSave] Term: cloud empty → migrating local to cloud.");
        }
    }
    //Add
    void SyncAccountKey(Dictionary<string, Item> loaded, PlayerAccountSaveProvider local)
    {
        bool hasCloud = loaded.TryGetValue(CloudSaveKeys.AccountSave, out Item cloudItem);
        string cloudJson = hasCloud ? GetJsonFromItem(cloudItem) : null;
        PlayerAccountData cloudData = !string.IsNullOrEmpty(cloudJson)
            ? JsonUtility.FromJson<PlayerAccountData>(cloudJson)
            : null;
        bool hasLocal = local.HasAccount();
        PlayerAccountData localData = hasLocal ? local.Load() : null;
        string playerId = AuthenticationService.Instance.PlayerId;
        if (cloudData != null && hasLocal)
        {
            if (cloudData.lastSavedUtc >= localData.lastSavedUtc)
            {
                local.Save(cloudData);
                Debug.Log("[CloudSave] Account: cloud newer → copied to PlayerPrefs.");
            }
            else
            {
                localData.playerId = playerId;
                QueueAccountSave(localData);
                Debug.Log("[CloudSave] Account: local newer → queued upload.");
            }
        }
        else if (cloudData != null)
        {
            local.Save(cloudData);
            Debug.Log("[CloudSave] Account: local empty → restored from cloud.");
        }
        else if (hasLocal && localData != null)
        {
            localData.playerId = playerId;
            QueueAccountSave(localData);
            Debug.Log("[CloudSave] Account: cloud empty → migrating local to cloud.");
        }
    }
    //Add — delete cloud keys on new game (optional but recommended)
    public async UniTask DeleteCloudKeysAsync(ISet<string> keys)
    {
        if (!CanUseCloudSave || keys == null || keys.Count == 0)
            return;
        foreach (string key in keys)
        {
            await CloudSaveService.Instance.Data.Player.DeleteAsync(key);
        }
        Debug.Log($"[CloudSave] Deleted {keys.Count} cloud key(s).");
    }
}