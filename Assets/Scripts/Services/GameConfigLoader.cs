using UnityEngine;

public class GameConfigLoader
{
    public static void ApplyTerm(GameConfigSO config, int maxDays)
    {
        int[] examDays = TryGetExamDaysFromRemoteConfig(maxDays)
                         ?? TermScheduleBuilder.BuildExamDays(maxDays);
        config.ApplyTerm(maxDays, examDays);
    }
    static int[] TryGetExamDaysFromRemoteConfig(int maxDays)
    {
        //TODO: For Remote Config Later
        return null;
    }
}
