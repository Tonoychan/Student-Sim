using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using UnityEngine;

public class CloudCurrencyService
{
    // singleton 
    public static CloudCurrencyService Instance { get; private set; }

    public CloudCurrencyService()
    {
        Instance = this;
    }

    public bool CanUseCloudCode =>
        AuthenticationService.Instance.IsSignedIn;

    /// <summary>
    /// Pull authoritative gold from server and apply to local currency service.
    /// </summary>
    public async UniTask<bool> SyncWalletAsync(PlayerCurrencyService currency)
    {
        if (!CanUseCloudCode || currency == null)
        {
            Debug.LogWarning("[CloudCurrency] Cannot sync wallet — not signed in.");
            return false;
        }

        try
        {
            var response = await CloudCodeService.Instance
                .CallEndpointAsync<CloudWalletResponse>(CloudCodeEndpoints.GetWallet, null);

            if (response == null || !response.success)
            {
                Debug.LogWarning($"[CloudCurrency] GetWallet failed: {response?.error}");
                return false;
            }

            currency.SetBalance(GameEnums.CurrencyType.Gold, response.gold);
            Debug.Log($"[CloudCurrency] Wallet synced. gold={response.gold}");
            return true;
        }
        catch (CloudCodeException ex)
        {
            Debug.LogError($"[CloudCurrency] GetWallet CloudCodeException: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CloudCurrency] GetWallet error: {ex.Message}");
            return false;
        }
    }

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

            if (!response.success)
                Debug.LogWarning($"[CloudCurrency] GrantGold rejected: {response.error}");

            return response;
        }
        catch (CloudCodeException ex)
        {
            Debug.LogError($"[CloudCurrency] GrantGold CloudCodeException: {ex.Message}");
            return FailGrant(ex.Message);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CloudCurrency] GrantGold error: {ex.Message}");
            return FailGrant(ex.Message);
        }
    }

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

            if (!response.success)
                Debug.LogWarning($"[CloudCurrency] SpendGold rejected: {response.error}");

            return response;
        }
        catch (CloudCodeException ex)
        {
            Debug.LogError($"[CloudCurrency] SpendGold CloudCodeException: {ex.Message}");
            return FailSpend(ex.Message);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CloudCurrency] SpendGold error: {ex.Message}");
            return FailSpend(ex.Message);
        }
    }

    static CloudGrantGoldResponse FailGrant(string error) =>
        new CloudGrantGoldResponse { success = false, error = error };

    static CloudSpendGoldResponse FailSpend(string error) =>
        new CloudSpendGoldResponse { success = false, error = error };
}