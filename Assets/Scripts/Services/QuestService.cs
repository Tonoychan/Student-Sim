using System.Collections.Generic;
using Cysharp.Threading.Tasks;

/// <summary>
/// Tracks the active quest and checks pass/fail at the end of each day.
/// Grants gold through the server when a quest is completed.
/// </summary>
public class QuestService
{
    private readonly QuestEntry[] _quests;
    private readonly PlayerStateService _playerState;
    private readonly PlayerCurrencyService _currency;
    private readonly CloudCurrencyService _cloudCurrency;
   
    private int _activeQuestIndex;
    
    public QuestEntry ActiveQuest => GetQuestAt(_activeQuestIndex);
    public int ActiveQuestIndex => _activeQuestIndex;
    public bool HasActiveQuest => ActiveQuest != null;
    
    public QuestService(
        QuestEntry[] quests,
        PlayerStateService playerState,
        PlayerCurrencyService currency,
        CloudCurrencyService cloudCurrency) 
    {
        _quests = quests ?? System.Array.Empty<QuestEntry>();
        _playerState = playerState;
        _currency = currency;
        _cloudCurrency = cloudCurrency;  
    }
    
    /// <summary>Notifies the UI of the current quest on load.</summary>
    public void Initialize()
    {
        ClampIndex();
        RaiseQuestUpdated();
    }
    
    /// <summary>Checks if the active quest passed or failed when a day ends.</summary>
    public void EvaluateActiveQuestAtDayEnd(int currentDay)
    {
        QuestEntry quest = ActiveQuest;
        if (quest == null)
            return;
        if (currentDay < quest.deadlineDay)
            return;
        int score = _playerState.GetSubjectScore(quest.subject);
        if (score >= quest.requiredScore)
            CompleteQuest(quest);
        else
            FailQuest(quest);
    }
    
    private void CompleteQuest(QuestEntry quest)
    {
        CompleteQuestAsync(quest).Forget();
    }
    
    async UniTaskVoid CompleteQuestAsync(QuestEntry quest)
    {
        if (_cloudCurrency == null || !_cloudCurrency.CanUseCloudCode)
        {
            GameEvents.RaiseQuestCompleted(quest, 0);
            AdvanceToNextQuest();
            return;
        }
        CloudGrantGoldResponse result = await _cloudCurrency.GrantGoldAsync(
            quest.questId,
            quest.goldReward);
        if (!result.success)
        {
            GameEvents.RaiseQuestCompleted(quest, 0);
            AdvanceToNextQuest();
            return;
        }
        _currency.SetBalance(GameEnums.CurrencyType.Gold, result.gold);
        GameEvents.RaiseQuestCompleted(quest, result.granted);
        AdvanceToNextQuest();
    }
    
    private void FailQuest(QuestEntry quest)
    {
        GameEvents.RaiseQuestFailed(quest);
        AdvanceToNextQuest();
    }
    
    private void AdvanceToNextQuest()
    {
        _activeQuestIndex++;
        ClampIndex();
        RaiseQuestUpdated();
    }
    
    private void ClampIndex()
    {
        if (_quests == null || _quests.Length == 0)
        {
            _activeQuestIndex = -1;
            return;
        }
        if (_activeQuestIndex >= _quests.Length)
            _activeQuestIndex = -1;
    }
    
    private void RaiseQuestUpdated()
    {
        GameEvents.RaiseActiveQuestChanged(ActiveQuest);
    }
    
    private QuestEntry GetQuestAt(int index)
    {
        if (_quests == null || index < 0 || index >= _quests.Length)
            return null;
        return _quests[index];
    }
   
    /// <summary>Saves which quest the player is on.</summary>
    public QuestSaveData ToSaveData()
    {
        return new QuestSaveData { activeQuestIndex = _activeQuestIndex };
    }

    /// <summary>Restores quest progress from save data.</summary>
    public void ApplySaveData(QuestSaveData data)
    {
        _activeQuestIndex = data?.activeQuestIndex ?? 0;
        ClampIndex();
    }
}
