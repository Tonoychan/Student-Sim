using Unity.Services.Authentication;

/// <summary>
/// Keeps wallet data tied to the current player ID.
/// Resets the wallet if a different player signs in.
/// </summary>
public class PlayerAccountService
{
    private readonly PlayerAccountSaveProvider _provider = new();
    
    /// <summary>Loads account save and applies wallet to the currency service.</summary>
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
            account = new PlayerAccountData { playerId = currentPlayerId };
            _provider.Save(account);
            currency.ApplySaveEntries(account.currencies);
            return;
        }
        currency.ApplySaveEntries(account.currencies);
    }
    
    /// <summary>Writes current wallet to account save.</summary>
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
