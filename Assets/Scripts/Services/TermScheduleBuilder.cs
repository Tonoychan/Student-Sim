using UnityEngine;

public class TermScheduleBuilder
{
    public static int[] BuildExamDays(int maxDays)
    {
        return maxDays switch
        {
            5 => new[] { 5 },
            30 => new[] { 7, 14, 21, 28, 30 },
            120 => new[] { 30, 60, 90, 120 },
            360 => new[] { 90, 180, 270, 360 },
            _ => new[] { maxDays }
        };
    }
}
