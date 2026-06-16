using UnityEngine;

public class TermLeaderboardIds
{
    public const string Term5Day = "term_5_Day";
    public const string Term30Day = "term_30_Day";
    
    public static string GetForMaxDays(int maxDays)
    {
        return maxDays switch
        {
            5 => Term5Day,
            30 => Term30Day,
            _ => null
        };
    }
    
    public static bool HasLeaderboard(int maxDays)
        => maxDays is 5 or 30;
}
