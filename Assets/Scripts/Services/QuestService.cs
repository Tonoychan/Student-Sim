using System.Collections.Generic;
using UnityEngine;

public class QuestService
{
    private readonly QuestData _questData;
    private readonly PlayerStateService _playerState;
    private readonly PlayerCurrencyService _currency;
   
    
    private int _activeQuestIndex;
    
    public QuestEntry ActiveQuest => GetQuestAt(_activeQuestIndex);
    public int ActiveQuestIndex => _activeQuestIndex;
    public bool HasActiveQuest => ActiveQuest != null;
    
    public QuestService(
        QuestData questData,
        PlayerStateService playerState,
        PlayerCurrencyService currency)
    {
        _questData = questData;
        _playerState = playerState;
        _currency = currency;
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
        _currency.Add(GameEnums.CurrencyType.Gold, quest.goldReward);
        Debug.Log($"[Quest] Completed '{quest.questId}'. Gold +{quest.goldReward}");
        GameEvents.RaiseQuestCompleted(quest, quest.goldReward);
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
        if (_questData?.quests == null || _questData.quests.Length == 0)
        {
            _activeQuestIndex = -1;
            return;
        }
        if (_activeQuestIndex >= _questData.quests.Length)
            _activeQuestIndex = -1; // all quests consumed
    }
    
    private void RaiseQuestUpdated()
    {
        GameEvents.RaiseActiveQuestChanged(ActiveQuest);
    }
    
    private QuestEntry GetQuestAt(int index)
    {
        if (_questData?.quests == null || index < 0 || index >= _questData.quests.Length)
            return null;
        return _questData.quests[index];
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
