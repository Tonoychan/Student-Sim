/// <summary>
/// Published Addressable addresses. Must match Unity Inspector addresses exactly.
/// </summary>
public static class AddressableKeys
{
    public const string SceneGame = "scene_game";

    public static string GetSubjectIconAddress(GameEnums.MainSubjects subject)
    {
        switch (subject)
        {
            case GameEnums.MainSubjects.Math:       return "icon_subject_math";
            case GameEnums.MainSubjects.History:    return "icon_subject_history";
            case GameEnums.MainSubjects.Science:    return "icon_subject_science";
            case GameEnums.MainSubjects.Geography:  return "icon_subject_geography";
            case GameEnums.MainSubjects.Arts:       return "icon_subject_arts";
            case GameEnums.MainSubjects.Computer:   return "icon_subject_computer";
            case GameEnums.MainSubjects.Rest:       return "icon_subject_rest";
            case GameEnums.MainSubjects.Work:       return "icon_subject_work";
            case GameEnums.MainSubjects.Exercise:   return "icon_subject_exercise";
            default:                                return null;
        }
    }
}