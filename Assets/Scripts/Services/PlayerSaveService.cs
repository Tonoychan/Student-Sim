using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;

/// <summary>
/// Handles saving and loading term progress to PlayerPrefs and the cloud.
/// </summary>
public class PlayerSaveService
{
    private readonly ISaveProvider _saveProvider;
    private readonly PlayerStateService _playerState;
    private readonly DayCycleService _dayCycleService;
    private readonly QuestService _questService;
    private readonly PlayerCurrencyService _currencyService;
    private readonly PlayerCloudSaveService _cloudSave;
    private readonly PlayerAccountService _accountService = new();
    
    public PlayerSaveService(
        ISaveProvider saveProvider,
        PlayerStateService playerState,
        DayCycleService dayCycleService,
        QuestService questService,
        PlayerCurrencyService currencyService,
        PlayerCloudSaveService cloudSave)  
    {
        _saveProvider = saveProvider;
        _playerState = playerState;
        _dayCycleService = dayCycleService;
        _questService = questService;
        _currencyService = currencyService;
        _cloudSave = cloudSave;
    }
    
    /// <summary>Loads existing save into all services, or starts with empty data.</summary>
    public void LoadOrCreateNew()
    {
        PlayerSaveData data = _saveProvider.HasSave()
            ? _saveProvider.Load()
            : new PlayerSaveData();
        _playerState.ApplySaveData(data);
        _currencyService.ApplySaveEntries(data.currencies);
        _questService.ApplySaveData(data.questProgress);
        _dayCycleService.SetCurrentDay(data.currentDay);
        _questService.Initialize();
    }
    
    /// <summary>Saves term progress locally and queues a cloud upload.</summary>
    public void Save(bool forceCloudFlush = false)
    {
        PlayerSaveData data = _playerState.ToSaveData(_dayCycleService.CurrentDay);
        data.currencies = _currencyService.ToSaveEntries();
        data.questProgress = _questService.ToSaveData();
        data.lastSavedUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        _saveProvider.Save(data);
        _cloudSave?.QueueTermSave(data);
        SaveAccountInternal();
        if (forceCloudFlush)
            _cloudSave?.ForceFlushAsync().Forget();
    }

    /// <summary>Wipes old save and starts a brand-new term.</summary>
    public void StartFresh(int maxDays)
    {
        _playerState.ResetTermProgress();
        _playerState.SetTermMaxDays(maxDays);
        _saveProvider.DeleteSave();
        _cloudSave?.DeleteCloudKeysAsync(new HashSet<string>
        {
            CloudSaveKeys.TermSave,
            CloudSaveKeys.AccountSave
        }).Forget();
        PlayerSaveData data = new PlayerSaveData { maxDays = maxDays };
        _playerState.ApplySaveData(data);
        _questService.ApplySaveData(data.questProgress);
        _dayCycleService.SetCurrentDay(data.currentDay);
        _questService.Initialize();
        Save(forceCloudFlush: true);
    }
    
    /// <summary>Loads wallet from account save on game start.</summary>
    public void SyncAccount(PlayerCurrencyService currency)
    {
        _accountService.SyncAndApply(currency);
    }

    /// <summary>Saves wallet to account save and optionally flushes to cloud.</summary>
    public void SaveAccount(bool forceCloudFlush = false)
    {
        SaveAccountInternal();
        if (forceCloudFlush)
            _cloudSave?.ForceFlushAsync().Forget();
    }
    
    void SaveAccountInternal()
    {
        _accountService.SaveFrom(_currencyService);
        if (_cloudSave == null || !_cloudSave.CanUseCloudSave)
            return;
        var account = new PlayerAccountData
        {
            playerId = AuthenticationService.Instance.PlayerId,
            currencies = _currencyService.ToSaveEntries(),
            lastSavedUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
        _cloudSave.QueueAccountSave(account);
    }
}
