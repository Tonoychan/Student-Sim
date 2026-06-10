using UnityEngine;

public class PlayerSaveService
{
    private readonly ISaveProvider _saveProvider;
    private readonly PlayerStateService _playerState;
    private readonly DayCycleService _dayCycleService;
    
    public PlayerSaveService(
        ISaveProvider saveProvider,
        PlayerStateService playerState,
        DayCycleService dayCycleService)
    {
        _saveProvider = saveProvider;
        _playerState = playerState;
        _dayCycleService = dayCycleService;
    }
    
    public void LoadOrCreateNew()
    {
        PlayerSaveData data = _saveProvider.HasSave()
            ? _saveProvider.Load()
            : new PlayerSaveData();
        _playerState.ApplySaveData(data);
        _dayCycleService.SetCurrentDay(data.currentDay);
    }
    
    public void Save()
    {
        PlayerSaveData data = _playerState.ToSaveData(_dayCycleService.CurrentDay);
        _saveProvider.Save(data);
    }
    public void StartFresh()
    {
        _saveProvider.DeleteSave();
        PlayerSaveData data = new PlayerSaveData();
        _playerState.ApplySaveData(data);
        _dayCycleService.SetCurrentDay(data.currentDay);
    }
}
