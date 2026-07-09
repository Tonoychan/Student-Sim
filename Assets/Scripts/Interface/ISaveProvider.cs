/// <summary>
/// Contract for reading and writing term save data (local or cloud).
/// </summary>
public interface ISaveProvider
{
    void Save(PlayerSaveData data);
    PlayerSaveData Load();
    bool HasSave();
    void DeleteSave();
}
