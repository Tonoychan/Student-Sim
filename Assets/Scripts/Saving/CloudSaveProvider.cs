using UnityEngine;

/// <summary>
/// Placeholder save provider for future direct cloud-only saves.
/// Currently unused — PlayerCloudSaveService handles cloud sync.
/// </summary>
public class CloudSaveProvider : ISaveProvider
{
    public void Save(PlayerSaveData data)
    {
        //Going to use Unity Gaming Service to Save
    }
    public PlayerSaveData Load()
    {
        // fetch remote, fallback to local if needed
        return new PlayerSaveData();
    }
    public bool HasSave() => true;
    public void DeleteSave() { }
}
