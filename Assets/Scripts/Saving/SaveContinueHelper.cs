using UnityEngine;

public class SaveContinueHelper
{
    public static bool CanContinue(ISaveProvider saveProvider)
    {
        if (!saveProvider.HasSave())
            return false;
        PlayerSaveData data = saveProvider.Load();
        return !data.termCompleted;
    }
}
