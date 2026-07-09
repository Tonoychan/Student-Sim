using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;

/// <summary>
/// Talks to Cloud Code endpoints for server-side gold (wallet, grant, spend).
/// The server is the source of truth for currency.
/// </summary>
public class CloudCurrencyService
{
    public static CloudCurrencyService Instance { get; private set; }

    public CloudCurrencyService()
    {
        Instance = this;
    }

    public bool CanUseCloudCode =>
        AuthenticationService.Instance.IsSignedIn;

    /// <summary>Pulls the player's gold from the server and updates local balance.</summary>
    public async UniTask<bool> SyncWalletAsync(PlayerCurrencyService currency)
    {
        if (!CanUseCloudCode || currency == null)
            return false;

        try
        {
            var response = await CloudCodeService.Instance
                .CallEndpointAsync<CloudWalletResponse>(CloudCodeEndpoints.GetWallet, null);

            if (response == null || !response.success)
                return false;

            currency.SetBalance(GameEnums.CurrencyType.Gold, response.gold);
            return true;
        }
        catch (CloudCodeException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>Asks the server to grant gold (e.g. quest reward). Blocks duplicate quest IDs.</summary>
    public async UniTask<CloudGrantGoldResponse> GrantGoldAsync(string questId, int goldReward)
    {
        if (!CanUseCloudCode)
            return FailGrant("NOT_SIGNED_IN");

        var args = new Dictionary<string, object>
        {
            { "questId", questId },
            { "goldReward", goldReward }
        };

        try
        {
            var response = await CloudCodeService.Instance
                .CallEndpointAsync<CloudGrantGoldResponse>(
                    CloudCodeEndpoints.GrantGold, args);

            if (response == null)
                return FailGrant("NULL_RESPONSE");

            return response;
        }
        catch (CloudCodeException ex)
        {
            return FailGrant(ex.Message);
        }
        catch (Exception ex)
        {
            return FailGrant(ex.Message);
        }
    }

    /// <summary>Asks the server to spend gold (e.g. store purchase).</summary>
    public async UniTask<CloudSpendGoldResponse> SpendGoldAsync(int amount, string reason = "store_purchase")
    {
        if (!CanUseCloudCode)
            return FailSpend("NOT_SIGNED_IN");

        var args = new Dictionary<string, object>
        {
            { "amount", amount },
            { "reason", reason }
        };

        try
        {
            var response = await CloudCodeService.Instance
                .CallEndpointAsync<CloudSpendGoldResponse>(
                    CloudCodeEndpoints.SpendGold, args);

            if (response == null)
                return FailSpend("NULL_RESPONSE");

            return response;
        }
        catch (CloudCodeException ex)
        {
            return FailSpend(ex.Message);
        }
        catch (Exception ex)
        {
            return FailSpend(ex.Message);
        }
    }

    static CloudGrantGoldResponse FailGrant(string error) =>
        new CloudGrantGoldResponse { success = false, error = error };

    static CloudSpendGoldResponse FailSpend(string error) =>
        new CloudSpendGoldResponse { success = false, error = error };
}
