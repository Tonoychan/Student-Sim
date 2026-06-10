using UnityEngine;

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
