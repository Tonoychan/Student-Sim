using UnityEngine;

[CreateAssetMenu(fileName = "GameConfigSO", menuName = "Advance Flow/GameConfigSO")]
public class GameConfigSO : ScriptableObject
{
    [Tooltip("Days when term exam replaces normal gameplay")]
    public int[] examDays = { 7, 14, 21 };
    public bool IsExamDay(int day)
    {
        foreach (int examDay in examDays)
            if (examDay == day) return true;
        return false;
    }
}
