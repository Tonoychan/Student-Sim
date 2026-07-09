/// <summary>
/// Cloud Save player-data keys. Add game keys here as you split saves into KVP.
/// </summary>
public static class CloudSaveKeys
{
    //Add — connectivity test (Phase 2a)
    public const string TestPing = "cloud_save_test_v1";

    //Add — reserved for Phase 2b (full game data)
    public const string TermSave = "term_save_v1";
    public const string AccountSave = "account_save_v1";
    public const string Wallet = "wallet_v1";
}