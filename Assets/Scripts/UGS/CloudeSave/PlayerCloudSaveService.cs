using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using UnityEngine;

/// <summary>
/// Saves and loads player data to Unity Cloud Save.
/// Keeps a local copy in PlayerPrefs and syncs in the background.
/// </summary>
public class PlayerCloudSaveService
{
    public static PlayerCloudSaveService Instance { get; private set; }

    private const int DebounceDelayMs = 35_000;

    private bool _termDirty;
    private bool _accountDirty;
    private string _pendingTermJson;
    private string _pendingAccountJson;
    private CancellationTokenSource _debounceCts;

    public PlayerCloudSaveService()
    {
        Instance = this;
    }

    /// <summary>True when the player is signed in and cloud save can be used.</summary>
    public bool CanUseCloudSave =>
        AuthenticationService.Instance.IsSignedIn;

    /// <summary>Uploads one or more key-value pairs to the cloud.</summary>
    public async UniTask SaveKeysAsync(Dictionary<string, object> data)
    {
        if (!CanUseCloudSave || data == null || data.Count == 0)
            return;

        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
    }

    /// <summary>Downloads the given keys from the cloud. Loads one key at a time for safety.</summary>
    public async UniTask<Dictionary<string, Item>> LoadKeysAsync(ISet<string> keys)
    {
        if (!CanUseCloudSave || keys == null || keys.Count == 0)
            return new Dictionary<string, Item>();

        var result = new Dictionary<string, Item>();
        foreach (string key in keys)
        {
            try
            {
                var singleKey = new HashSet<string> { key };
                var loaded = await CloudSaveService.Instance.Data.Player.LoadAsync(singleKey);
                if (loaded.TryGetValue(key, out Item item))
                    result[key] = item;
            }
            catch (CloudSaveException)
            {
                await TryDeleteCorruptKeyAsync(key);
            }
            catch (Exception)
            {
                await TryDeleteCorruptKeyAsync(key);
            }
        }

        return result;
    }

    /// <summary>Removes a broken cloud entry so local data can take over.</summary>
    async UniTask TryDeleteCorruptKeyAsync(string key)
    {
        try
        {
            await CloudSaveService.Instance.Data.Player.DeleteAsync(key);
        }
        catch (Exception)
        {
        }
    }

    /// <summary>Marks term progress dirty and schedules an upload.</summary>
    public void QueueTermSave(PlayerSaveData data)
    {
        if (!CanUseCloudSave || data == null)
            return;
        data.lastSavedUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        _pendingTermJson = JsonUtility.ToJson(data);
        _termDirty = true;
        ScheduleDebouncedFlush().Forget();
    }

    /// <summary>Marks account/wallet data dirty and schedules an upload.</summary>
    public void QueueAccountSave(PlayerAccountData data)
    {
        if (!CanUseCloudSave || data == null)
            return;
        data.lastSavedUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        _pendingAccountJson = JsonUtility.ToJson(data);
        _accountDirty = true;
        ScheduleDebouncedFlush().Forget();
    }

    /// <summary>Waits 35 seconds then uploads any pending changes.</summary>
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
        }
    }

    /// <summary>Uploads pending changes right away (used on app pause/quit).</summary>
    public async UniTask ForceFlushAsync()
    {
        _debounceCts?.Cancel();
        await FlushDirtyKeysAsync();
    }

    /// <summary>Uploads only the keys that changed since last flush.</summary>
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
    }

    static string GetJsonFromItem(Item item)
    {
        if (item == null || item.Value == null)
            return null;
        return item.Value.GetAsString();
    }

    /// <summary>
    /// Called at login. Picks the newer copy (cloud or local) for each save key
    /// and uploads anything that only exists locally.
    /// </summary>
    public async UniTask SyncCloudToLocalAsync()
    {
        if (!CanUseCloudSave)
            return;

        try
        {
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
            await FlushDirtyKeysAsync();
        }
        catch (Exception)
        {
        }
    }

    /// <summary>Merges cloud term save with local PlayerPrefs.</summary>
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
                local.Save(cloudData);
            else
                QueueTermSave(localData);
        }
        else if (cloudData != null)
        {
            local.Save(cloudData);
        }
        else if (hasLocal && localData != null && !localData.termCompleted)
        {
            QueueTermSave(localData);
        }
    }

    /// <summary>Merges cloud account save with local PlayerPrefs.</summary>
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
                local.Save(cloudData);
            else
            {
                localData.playerId = playerId;
                QueueAccountSave(localData);
            }
        }
        else if (cloudData != null)
        {
            local.Save(cloudData);
        }
        else if (hasLocal && localData != null)
        {
            localData.playerId = playerId;
            QueueAccountSave(localData);
        }
    }

    /// <summary>Deletes cloud keys (used when starting a new game).</summary>
    public async UniTask DeleteCloudKeysAsync(ISet<string> keys)
    {
        if (!CanUseCloudSave || keys == null || keys.Count == 0)
            return;
        foreach (string key in keys)
            await CloudSaveService.Instance.Data.Player.DeleteAsync(key);
    }
}
