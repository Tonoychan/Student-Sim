public interface ISaveProvider
{
    void Save(PlayerSaveData data);
    PlayerSaveData Load();
    bool HasSave();
    void DeleteSave();
}
