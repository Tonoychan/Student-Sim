using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Cysharp.Threading.Tasks;
using UnityEngine;

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
    
    public void Save(bool forceCloudFlush = false)
    {
        PlayerSaveData data = _playerState.ToSaveData(_dayCycleService.CurrentDay);
        data.currencies = _currencyService.ToSaveEntries();
        data.questProgress = _questService.ToSaveData();
        //Add
        data.lastSavedUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        _saveProvider.Save(data);
        //Add — queue cloud term key
        _cloudSave?.QueueTermSave(data);
        SaveAccountInternal();
        //Add
        if (forceCloudFlush)
            _cloudSave?.ForceFlushAsync().Forget();
    }
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
    
    public void SyncAccount(PlayerCurrencyService currency)
    {
        _accountService.SyncAndApply(currency);
    }
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
            playerId = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId,
            currencies = _currencyService.ToSaveEntries(),
            lastSavedUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
        _cloudSave.QueueAccountSave(account);
    }
}
