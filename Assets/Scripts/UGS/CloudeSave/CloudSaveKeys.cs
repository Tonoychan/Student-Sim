/// <summary>
/// String keys used when saving data to Unity Cloud Save.
/// </summary>
public static class CloudSaveKeys
{
    /// <summary>Full term progress (day, scores, quests, etc.).</summary>
    public const string TermSave = "term_save_v1";

    /// <summary>Account data tied to the player (wallet, player ID).</summary>
    public const string AccountSave = "account_save_v1";

    /// <summary>Reserved for future server-side wallet sync.</summary>
    public const string Wallet = "wallet_v1";
}
