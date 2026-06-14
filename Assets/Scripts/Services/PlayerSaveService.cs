using UnityEngine;

public class PlayerSaveService
{
    private readonly ISaveProvider _saveProvider;
    private readonly PlayerStateService _playerState;
    private readonly DayCycleService _dayCycleService;
    private readonly QuestService _questService;
    private readonly PlayerCurrencyService _currencyService;
    
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
    }
    public void StartFresh()
    {
        _saveProvider.DeleteSave();
        PlayerSaveData data = new PlayerSaveData();
        _playerState.ApplySaveData(data);
        _currencyService.ApplySaveEntries(data.currencies);  // empty → 0 gold
        _questService.ApplySaveData(data.questProgress);     // index 0
        _dayCycleService.SetCurrentDay(data.currentDay);
        _questService.Initialize(); // show first quest + fire UI event
    }
}
