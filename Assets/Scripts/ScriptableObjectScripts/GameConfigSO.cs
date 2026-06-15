using UnityEngine;

[CreateAssetMenu(fileName = "GameConfigSO", menuName = "Advance Flow/GameConfigSO")]
public class GameConfigSO : ScriptableObject
{
    public int maxDays = 30;
    public int[] examDays = {15,30};
    
    public bool IsExamDay(int day)
    {
        foreach (int examDay in examDays)
            if (examDay == day) return true;
        return false;
    }
    
    public void ApplyTerm(int maxDays, int[] examDays)
    {
        this.maxDays = maxDays;
        this.examDays = examDays;
    }
    
    public bool IsFinalDay(int day) => day >= maxDays;
    public int MaxPossibleExamCorrect => examDays.Length * 6;
}
