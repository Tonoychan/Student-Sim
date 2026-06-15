using UnityEngine;

public class PlayerAccountSaveProvider
{
    private const string AccountKey = "player_account_v1";
    public bool HasAccount() => PlayerPrefs.HasKey(AccountKey);
    
    public PlayerAccountData Load()
    {
        if (!HasAccount())
            return new PlayerAccountData();
        string json = PlayerPrefs.GetString(AccountKey);
        return JsonUtility.FromJson<PlayerAccountData>(json) ?? new PlayerAccountData();
    }
    public void Save(PlayerAccountData data)
    {
        PlayerPrefs.SetString(AccountKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }
    public void Delete()
    {
        PlayerPrefs.DeleteKey(AccountKey);
    }
}
