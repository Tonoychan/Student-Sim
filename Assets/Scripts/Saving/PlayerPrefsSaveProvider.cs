using UnityEngine;

public class PlayerPrefsSaveProvider : ISaveProvider
{
    private const string SaveKey = "player_save_v1";
    public void Save(PlayerSaveData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
        
    }
    public PlayerSaveData Load()
    {
        if (!HasSave())
            return new PlayerSaveData();
        string json = PlayerPrefs.GetString(SaveKey);
        Debug.Log($"LOAD JSON: {json}");
        return JsonUtility.FromJson<PlayerSaveData>(json);
    }
    public bool HasSave()
    {
        return PlayerPrefs.HasKey(SaveKey);
    }
    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
    }
}
