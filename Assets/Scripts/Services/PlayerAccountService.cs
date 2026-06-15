using Unity.Services.Authentication;
using UnityEngine;

public class PlayerAccountService
{
    private readonly PlayerAccountSaveProvider _provider = new();
    
    public void SyncAndApply(PlayerCurrencyService currency)
    {
        string currentPlayerId = AuthenticationService.Instance.IsSignedIn
            ? AuthenticationService.Instance.PlayerId
            : string.Empty;
        PlayerAccountData account = _provider.Load();
        if (string.IsNullOrEmpty(currentPlayerId))
        {
            currency.ApplySaveEntries(account.currencies);
            return;
        }
        if (account.playerId != currentPlayerId)
        {
            Debug.Log($"[Account] New player ID. Resetting wallet. Old={account.playerId}, New={currentPlayerId}");
            account = new PlayerAccountData { playerId = currentPlayerId };
            _provider.Save(account);
            currency.ApplySaveEntries(account.currencies);
            return;
        }
        currency.ApplySaveEntries(account.currencies);
    }
    
    public void SaveFrom(PlayerCurrencyService currency)
    {
        string currentPlayerId = AuthenticationService.Instance.IsSignedIn
            ? AuthenticationService.Instance.PlayerId
            : string.Empty;
        if (string.IsNullOrEmpty(currentPlayerId))
            return;
        _provider.Save(new PlayerAccountData
        {
            playerId = currentPlayerId,
            currencies = currency.ToSaveEntries()
        });
    }
}
