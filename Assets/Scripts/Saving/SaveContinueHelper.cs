/// <summary>
/// Checks whether the player has a save that can be continued.
/// </summary>
public class SaveContinueHelper
{
    /// <summary>True if a save exists and the term is not finished yet.</summary>
    public static bool CanContinue(ISaveProvider saveProvider)
    {
        if (!saveProvider.HasSave())
            return false;
        PlayerSaveData data = saveProvider.Load();
        return !data.termCompleted;
    }
}
