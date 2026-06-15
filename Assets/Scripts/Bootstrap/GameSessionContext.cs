using UnityEngine;

public class GameSessionContext
{
    public enum StartMode
    {
        NewGame,
        Continue
    }
    
    public static StartMode Mode { get; private set; } = StartMode.NewGame;
    public static int SelectedMaxDays { get; private set; } = 30;
    
    public static void StartNewGame(int maxDays)
    {
        Mode = StartMode.NewGame;
        SelectedMaxDays = maxDays;
    }
    public static void ContinueGame()
    {
        Mode = StartMode.Continue;
    }
    public static void Reset()
    {
        Mode = StartMode.NewGame;
        SelectedMaxDays = 30;
    }
}
