using UnityEngine;
/// <summary>
/// Using this class as a Global class for the Enums
/// </summary>
public class GameEnums
{
    public enum MainSubjects
    {
        None = 0,
        Math,
        History,
        Science,
        Geography,
        Arts,
        Computer,
        Rest,
        Work,
        Exercise,
    }
    
    public enum CurrencyType
    {
        Gold=0
    }
    
    public enum QuestStatus
    {
        Active,
        Completed,
        Failed   // deadline passed, score not met
    }
}
