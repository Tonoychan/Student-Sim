using UnityEngine;

public class PlayerSaveService
{
    private readonly ISaveProvider _saveProvider;
    private readonly PlayerStateService _playerState;
    private readonly DayCycleService _dayCycleService;
    private readonly QuestService _questService;
    private readonly PlayerCurrencyService _currencyService;
    private readonly PlayerAccountService _accountService = new();
    
    public PlayerSaveService(
        ISaveProvider saveProvider,
        PlayerStateService playerState,
        DayCycleService dayCycleService,
        QuestService questService,
        PlayerCurrencyService currencyService)
    {
        _saveProvider = saveProvider;
        _playerState = playerState;
        _dayCycleService = dayCycleService;
        _questService = questService;
        _currencyService = currencyService;
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
    
    public void Save()
    {
        PlayerSaveData data = _playerState.ToSaveData(_dayCycleService.CurrentDay);
        data.currencies = _currencyService.ToSaveEntries();
        data.questProgress = _questService.ToSaveData();
        _saveProvider.Save(data);
        _accountService.SaveFrom(_currencyService);
    }
    public void StartFresh(int maxDays)
    {
        _playerState.ResetTermProgress();
        _playerState.SetTermMaxDays(maxDays);
        _saveProvider.DeleteSave();
        PlayerSaveData data = new PlayerSaveData { maxDays = maxDays };
        _playerState.ApplySaveData(data);
        _questService.ApplySaveData(data.questProgress);
        _dayCycleService.SetCurrentDay(data.currentDay);
        _questService.Initialize();
        Save();
    }
    
    public void SyncAccount(PlayerCurrencyService currency)
    {
        _accountService.SyncAndApply(currency);
    }
    public void SaveAccount()
    {
        _accountService.SaveFrom(_currencyService);
    }
}
