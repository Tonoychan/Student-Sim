using UnityEngine;

/// <summary>
/// Holds info passed from the selection scene to the game scene
/// (new game vs continue, and how many days the term lasts).
/// </summary>
public class GameSessionContext
{
    public enum StartMode
    {
        NewGame,
        Continue
    }
    
    public static StartMode Mode { get; private set; } = StartMode.NewGame;
    public static int SelectedMaxDays { get; private set; } = 30;
    
    /// <summary>Start a fresh term with the chosen number of days.</summary>
    public static void StartNewGame(int maxDays)
    {
        Mode = StartMode.NewGame;
        SelectedMaxDays = maxDays;
    }

    /// <summary>Resume the saved term in progress.</summary>
    public static void ContinueGame()
    {
        Mode = StartMode.Continue;
    }

    /// <summary>Clear session data (e.g. after returning to menu).</summary>
    public static void Reset()
    {
        Mode = StartMode.NewGame;
        SelectedMaxDays = 30;
    }
}
