using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

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
    
    public void Initialize()
    {
        ClampIndex();
        RaiseQuestUpdated();
    }
    
    /// Call when a day ends (same moment legacy called CheckGoalAchieved)
    public void EvaluateActiveQuestAtDayEnd(int currentDay)
    {
        QuestEntry quest = ActiveQuest;
        if (quest == null)
            return;
        // Not past deadline yet — keep showing same quest
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
            Debug.LogWarning($"[Quest] Offline — cannot grant gold for '{quest.questId}'. Quest still advances.");
            GameEvents.RaiseQuestCompleted(quest, 0);
            AdvanceToNextQuest();
            return;
        }
        CloudGrantGoldResponse result = await _cloudCurrency.GrantGoldAsync(
            quest.questId,
            quest.goldReward);
        if (!result.success)
        {
            Debug.LogWarning($"[Quest] GrantGold failed for '{quest.questId}': {result.error}");
            // Still advance — player earned the quest; gold may already be claimed server-side
            GameEvents.RaiseQuestCompleted(quest, 0);
            AdvanceToNextQuest();
            return;
        }
        _currency.SetBalance(GameEnums.CurrencyType.Gold, result.gold);
        Debug.Log($"[Quest] Completed '{quest.questId}'. Server gold={result.gold} (+{result.granted})");
        GameEvents.RaiseQuestCompleted(quest, result.granted);
        AdvanceToNextQuest();
    }
    
    private void FailQuest(QuestEntry quest)
    {
        Debug.Log($"[Quest] Failed '{quest.questId}'. Required {quest.requiredScore}");
        GameEvents.RaiseQuestFailed(quest);
        AdvanceToNextQuest();  // same advance, no gold
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
   
    // ---------- SAVE ----------
    public QuestSaveData ToSaveData()
    {
        return new QuestSaveData { activeQuestIndex = _activeQuestIndex };
    }
    public void ApplySaveData(QuestSaveData data)
    {
        _activeQuestIndex = data?.activeQuestIndex ?? 0;
        ClampIndex();
    }
}
